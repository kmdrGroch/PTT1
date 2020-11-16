﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PT_Task1.DataLayer;
using PT_Task1.LogicLayer;

namespace Task1_Test
{
    [TestClass]
    public class LibraryServiceTest
    {
        Library l;
        LibraryService ls;

        [TestInitialize]
        public void BeforeEach()
        {
            l = new Library();
            ls = new LibraryService(l);
        }

        [TestMethod]
        public void RentBook_Test()
        {
            ls.Login("White");
            Assert.ThrowsException<LibraryService.NoSuchEntry_Exception>(() => ls.RentBook("On the Bright Side", "Hendrik Groen", true));

            ls.RentBook("On the Bright Side", "Hendrik Groen", false);
            l.SelectBook("On the Bright Side", "Hendrik Groen", false, Book.BookState.BORROWED, "White");
            Assert.AreEqual(l.eventHistory[0].type, Event.EventType.RENT_A_BOOK);
            Assert.AreEqual(l.eventHistory[0].bookAffected.Title, "On the Bright Side");
            Assert.AreEqual(l.eventHistory[0].actor.Username, "White");

            Assert.ThrowsException<LibraryService.NonExistingBook_Exception>(() => ls.RentBook("On the Bright Side", "Hendrik Groen", false));
        }

        [TestMethod]
        public void RentBook_UserNotAllowedToBorrow_Test()
        {
            ls.Login("Red");
            Assert.ThrowsException<LibraryService.NotAppropriatePermits_Exception>(() => ls.RentBook("On the Bright Side", "Hendrik Groen", false));
            Assert.AreEqual(l.eventHistory.Count, 0);
        }

        [TestMethod]
        public void RentBook_UserTriesToExceedTheirLimit_Test()
        {
            ls.Login("White");
            for (int i = 1; i <= 6; i++)
            {
                ls.RentBook("Pride and Prejudice", "Jane Austin", false);
                Assert.AreEqual(l.eventHistory[i - 1].type, Event.EventType.RENT_A_BOOK);
                Assert.AreEqual(l.eventHistory[i - 1].actor.Username, "White");
                Assert.AreEqual(l.eventHistory[i - 1].bookAffected.Title, "Pride and Prejudice");
            }
            Assert.ThrowsException<LibraryService.NotAppropriatePermits_Exception>(() => ls.RentBook("Pride and Prejudice", "Jane Austin", false));
        }

        [TestMethod]
        public void RentBook_NonExistingBook_Test()
        {
            ls.Login("White");
            Assert.ThrowsException<LibraryService.NonExistingBook_Exception>(() => ls.RentBook("Harry Potter and the Philosopher's Stone", "J. K. Rowling", true));
            Assert.AreEqual(l.eventHistory.Count, 0);
        }

        [TestMethod]
        public void ReturnBook_Test()
        {
            ls.Login("White");
            ls.RentBook("Pride and Prejudice", "Jane Austin", false);
            l.SelectBook("Pride and Prejudice", "Jane Austin", false, Book.BookState.BORROWED, "White");

            ls.ReturnBook("Pride and Prejudice", "Jane Austin", false);
            Assert.AreEqual(l.eventHistory[1].type, Event.EventType.BOOK_RETURN);
            Assert.AreEqual(l.eventHistory[1].actor.Username, "White");
            Assert.AreEqual(l.eventHistory[1].bookAffected.Title, "Pride and Prejudice");

            Assert.ThrowsException<ILibrary.NoSuchBook_Exception>(()
                => l.SelectBook("Pride and Prejudice", "Jane Austin", false, Book.BookState.BORROWED, "White"));
        }

        [TestMethod]
        public void ReturnBook_UserNotOwningTheBook_Test()
        {
            ls.Login("Black");
            Assert.ThrowsException<LibraryService.NonExistingBook_Exception>(() => ls.ReturnBook("Pride and Prejudice", "Jane Austin", false));
            Assert.AreEqual(l.eventHistory.Count, 0);
        }

        [TestMethod]
        public void ReturnBook_MultipleInstances_Test()
        {
            ls.Login("White");
            ls.RentBook("Pride and Prejudice", "Jane Austin", false);

            ls.Login("Black");
            ls.RentBook("Pride and Prejudice", "Jane Austin", false);

            l.SelectBook("Pride and Prejudice", "Jane Austin", false, Book.BookState.BORROWED, "White");
            l.SelectBook("Pride and Prejudice", "Jane Austin", false, Book.BookState.BORROWED, "Black");

            ls.ReturnBook("Pride and Prejudice", "Jane Austin", false);

            Assert.AreEqual(l.eventHistory[2].type, Event.EventType.BOOK_RETURN);
            Assert.AreEqual(l.eventHistory[2].actor.Username, "Black");
            Assert.AreEqual(l.eventHistory[2].bookAffected.Title, "Pride and Prejudice");
            l.SelectBook("Pride and Prejudice", "Jane Austin", false, Book.BookState.BORROWED, "White");
            Assert.ThrowsException<ILibrary.NoSuchBook_Exception>(() =>
                l.SelectBook("Pride and Prejudice", "Jane Austin", false, Book.BookState.BORROWED, "Black"));
        }

        [TestMethod]
        public void ReserveBook_Test()
        {
            ls.Login("White");
            ls.ReserveBook("Pride and Prejudice", "Jane Austin", false);
            Assert.AreEqual(l.eventHistory[0].type, Event.EventType.RESERVATION);
            Assert.AreEqual(l.eventHistory[0].actor.Username, "White");
            Assert.AreEqual(l.eventHistory[0].bookAffected.Title, "Pride and Prejudice");
            l.SelectBook("Pride and Prejudice", "Jane Austin", false, Book.BookState.RESERVED, "White");
        }

        [TestMethod]
        public void ReserveBook_UserNotAllowedToReserve_Test()
        {
            ls.Login("Blue");
            Assert.ThrowsException<LibraryService.NonExistingBook_Exception>(() => ls.ReturnBook("Pride and Prejudice", "Jane Austin", false));
        }

        [TestMethod]
        public void ReserveBook_Queue1_Test()
        {
            ls.Login("White");
            ls.ReserveBook("On the Bright Side", "Hendrik Groen", false);

            ls.Login("Black");
            ls.ReserveBook("On the Bright Side", "Hendrik Groen", false);

            ls.Login("White");
            ls.RentReservedBook("On the Bright Side", "Hendrik Groen", false);
            Assert.AreEqual(l.eventHistory[2].type, Event.EventType.RENT_A_BOOK);
            Assert.AreEqual(l.eventHistory[2].actor.Username, "White");
            Assert.AreEqual(l.eventHistory[2].bookAffected.Title, "On the Bright Side");

            ls.ReturnBook("On the Bright Side", "Hendrik Groen", false);
            l.SelectBook("On the Bright Side", "Hendrik Groen", false, Book.BookState.RESERVED, "Black");
        }

        [TestMethod]
        public void ReserveBook_Queue2_Test()
        {
            ls.Login("Black");
            ls.ReserveBook("On the Bright Side", "Hendrik Groen", false);

            ls.Login("Blue");
            Assert.ThrowsException<LibraryService.NonExistingBook_Exception>(() => ls.RentBook("On the Bright Side", "Hendrik Groen", false));
            Assert.AreEqual(l.eventHistory.Count, 1);
        }

        [TestMethod]
        public void ReserveBook_Queue3_Test()
        {
            ls.Login("Black");
            ls.ReserveBook("On the Bright Side", "Hendrik Groen", false);
            ls.RentReservedBook("On the Bright Side", "Hendrik Groen", false);

            ls.Login("White");
            l.SelectBook("On the Bright Side", "Hendrik Groen", false, Book.BookState.BORROWED, "Black");
            ls.ReserveBook("On the Bright Side", "Hendrik Groen", false);

            ls.Login("Black");
            ls.ReturnBook("On the Bright Side", "Hendrik Groen", false);
            l.SelectBook("On the Bright Side", "Hendrik Groen", false, Book.BookState.RESERVED, "White");
            Assert.AreEqual(l.eventHistory[3].type, Event.EventType.BOOK_RETURN);
            Assert.AreEqual(l.eventHistory[3].actor.Username, "Black");
            Assert.AreEqual(l.eventHistory[3].bookAffected.Title, "On the Bright Side");
            Assert.AreEqual(l.eventHistory.Count, 4);
        }

        [TestMethod]
        public void ReserveBook_NonExistingBook_Test()
        {
            ls.Login("White");
            Assert.ThrowsException<LibraryService.NonExistingBook_Exception>(() => ls.ReserveBook("Harry Potter and the Philosopher's Stone", "J. K. Rowling", true));
            Assert.AreEqual(l.eventHistory.Count, 0);
        }

        [TestMethod]
        public void AddAndRemoveBooks_Test()
        {
            ls.Login("Gold");
            Assert.ThrowsException<ILibrary.NoSuchBook_Exception>(() =>
                l.SelectBook("Harry Potter and the Philosopher's Stone", "J. K. Rowling", true, Book.BookState.AVAILABLE));

            int temp = l.bookList.Count;
            ls.AddBook("Harry Potter and the Philosopher's Stone", "J. K. Rowling", true);
            Assert.AreEqual(l.eventHistory[0].type, Event.EventType.ADD_A_BOOK);
            Assert.AreEqual(l.eventHistory[0].actor.Username, "Gold");
            Assert.AreEqual(l.eventHistory[0].bookAffected.Title, "Harry Potter and the Philosopher's Stone");

            l.SelectBook("Harry Potter and the Philosopher's Stone", "J. K. Rowling", true, Book.BookState.AVAILABLE);
            Assert.AreEqual(l.bookList.Count, temp + 1);

            ls.RemoveBook("On the Bright Side", "Hendrik Groen", false);
            Assert.AreEqual(l.eventHistory[1].type, Event.EventType.REMOVE_A_BOOK);
            Assert.AreEqual(l.eventHistory[1].actor.Username, "Gold");
            Assert.AreEqual(l.eventHistory[1].bookAffected.Title, "On the Bright Side");
            Assert.ThrowsException<ILibrary.NoSuchBook_Exception>(() =>
                l.SelectBook("On the Bright Side", "Hendrik Groen", false, Book.BookState.AVAILABLE));
        }

        [TestMethod]
        public void AddAndRemoveEntries_Test()
        {
            ls.Login("Gold");
            Assert.IsFalse(l.CheckIfEntryExists("The Tempest", "William Shakespeare", false));
            int temp = l.bookList.Count;

            ls.AddCatalogEntry("The Tempest", "William Shakespeare", false);
            Assert.IsTrue(l.CheckIfEntryExists("The Tempest", "William Shakespeare", false));

            ls.AddBook("The Tempest", "William Shakespeare", false);
            ls.AddBook("The Tempest", "William Shakespeare", false);

            Assert.AreEqual(l.bookList.Count, temp + 2);

            ls.RemoveCatalogEntry("The Tempest", "William Shakespeare", false);

            Assert.AreEqual(l.eventHistory[2].type, Event.EventType.REMOVE_A_BOOK);
            Assert.AreEqual(l.eventHistory[2].actor.Username, "Gold");
            Assert.AreEqual(l.eventHistory[2].bookAffected.Title, "The Tempest");

            Assert.AreEqual(l.eventHistory[3].type, Event.EventType.REMOVE_A_BOOK);
            Assert.AreEqual(l.eventHistory[3].actor.Username, "Gold");
            Assert.AreEqual(l.eventHistory[3].bookAffected.Title, "The Tempest");

            Assert.AreEqual(l.bookList.Count, temp);
            Assert.IsFalse(l.CheckIfEntryExists("The Tempest", "William Shakespeare", false));
        }
    }
}
