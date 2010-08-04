﻿// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace UnitTest.API
{
    [TestFixture]
    public class UtilitiesTests : TestFixtureBase
    {
        [Test]
        [Description("Import a message using the mail importer")]
        public void TestImportOfMessageIntoInbox()
        {
            string @messageText =
               "From: test@test.com\r\n" +
               "\r\n" +
               "Test\r\n";

            hMailServer.Account account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test");

            string domainPath = Path.Combine(_application.Settings.Directories.DataDirectory, "test.com");
            string accountPath = Path.Combine(domainPath, "test");

            Directory.CreateDirectory(accountPath);

            string fileName = Path.Combine(accountPath, "something.eml");

            File.WriteAllText(fileName, messageText);

            Assert.IsTrue(_application.Utilities.ImportMessageFromFile(fileName, account.ID));

            string text = POP3Simulator.AssertGetFirstMessageText("test@test.com", "test");
            Assert.IsTrue(text.Contains(messageText));
        }

        [Test]
        [Description("Import a message using the mail importer")]
        public void TestImportOfMessageIntoInbox2()
        {
            string @messageText =
               "From: test@test.com\r\n" +
               "\r\n" +
               "Test\r\n";

            hMailServer.Account account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test");

            string domainPath = Path.Combine(_application.Settings.Directories.DataDirectory, "test.com");
            string accountPath = Path.Combine(domainPath, "test");

            Directory.CreateDirectory(accountPath);

            string fileName = Path.Combine(accountPath, "something.eml");

            File.WriteAllText(fileName, messageText);

            Assert.IsTrue(_application.Utilities.ImportMessageFromFileToIMAPFolder(fileName, account.ID, "Inbox"));

            string text = POP3Simulator.AssertGetFirstMessageText("test@test.com", "test");
            Assert.IsTrue(text.Contains(messageText));
        }

        [Test]
        [Description("Import a message using the mail importer")]
        public void TestImportOfMessageIntoOtherFolder()
        {
            string @messageText =
               "From: test@test.com\r\n" +
               "\r\n" +
               "Test\r\n";

            hMailServer.Account account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test");

            account.IMAPFolders.Add("Woho");

            string domainPath = Path.Combine(_application.Settings.Directories.DataDirectory, "test.com");
            string accountPath = Path.Combine(domainPath, "test");

            Directory.CreateDirectory(accountPath);
            string fileName = Path.Combine(accountPath, "something.eml");

            File.WriteAllText(fileName, messageText);

            Assert.IsTrue(_application.Utilities.ImportMessageFromFileToIMAPFolder(fileName, account.ID, "Woho"));

            POP3Simulator.AssertMessageCount("test@test.com", "test", 0);
            IMAPSimulator sim = new IMAPSimulator();
            sim.ConnectAndLogon("test@test.com", "test");
            Assert.AreEqual(1, sim.GetMessageCount("Woho"));
            sim.Disconnect();
        }

        [Test]
        [Description("Import a mail located properly in a sub directory.")]
        public void TestImportOfMessageInSubdirectory()
        {
            string @messageText =
               "From: test@test.com\r\n" +
               "\r\n" +
               "Test\r\n";

            hMailServer.Account account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test");

            string domainPath = Path.Combine(_application.Settings.Directories.DataDirectory, "test.com");
            string accountPath = Path.Combine(domainPath, "test");
            Directory.CreateDirectory(accountPath);

            string guid = Guid.NewGuid().ToString();
            string guidPath = Path.Combine(accountPath, guid.Substring(1, 2));
            Directory.CreateDirectory(guidPath);

            string fileName = Path.Combine(guidPath, guid +".eml");

            File.WriteAllText(fileName, messageText);

            Assert.IsTrue(_application.Utilities.ImportMessageFromFile(fileName, account.ID));

            hMailServer.Message message = _domain.Accounts[0].IMAPFolders.get_ItemByName("Inbox").Messages[0];
            Assert.AreEqual(fileName, message.Filename);
        }

        [Test]
        [Description("Import a mail located properly in a sub directory.")]
        public void TestImportOfMessageInInvalidSubName()
        {
            string @messageText =
               "From: test@test.com\r\n" +
               "\r\n" +
               "Test\r\n";

            hMailServer.Account account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test");

            string domainPath = Path.Combine(_application.Settings.Directories.DataDirectory, "test.com");
            string accountPath = Path.Combine(domainPath, "test");
            Directory.CreateDirectory(accountPath);

            string guid = Guid.NewGuid().ToString();
            string guidPath = Path.Combine(accountPath, guid.Substring(1, 2));
            Directory.CreateDirectory(guidPath);

            string fileName = Path.Combine(guidPath, "§§§§.eml");

            File.WriteAllText(fileName, messageText);

            Assert.IsTrue(_application.Utilities.ImportMessageFromFile(fileName, account.ID));

            hMailServer.Message message = _domain.Accounts[0].IMAPFolders.get_ItemByName("Inbox").Messages[0];
            Assert.IsFalse(fileName.Contains("$$$$.eml"));
        }

        [Test]
        [Description("Import all messages in public folders. This must fail, since we don't know what public folder to put it into.")]
        public void TestImportOfMessageInPublicFolder()
        {
            string @messageText =
               "From: test@test.com\r\n" +
               "\r\n" +
               "Test\r\n";

            hMailServer.Account account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test");
            string publicFolder = Path.Combine(_application.Settings.Directories.DataDirectory, "#Public");

            if (!Directory.Exists(publicFolder))
                Directory.CreateDirectory(publicFolder);

            string fileName = Path.Combine(publicFolder, "§§§§.eml");

            File.WriteAllText(fileName, messageText);

            Assert.IsFalse(_application.Utilities.ImportMessageFromFile(fileName, account.ID));
        }

        [Test]
        [Description("Import the same message twice.")]
        public void TestImportDuplicateMessage()
        {
            string @messageText =
               "From: test@test.com\r\n" +
               "\r\n" +
               "Test\r\n";

            hMailServer.Account account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test");

            string domainPath = Path.Combine(_application.Settings.Directories.DataDirectory, "test.com");
            string accountPath = Path.Combine(domainPath, "test");

            Directory.CreateDirectory(accountPath);

            string fileName = Path.Combine(accountPath, "something.eml");

            File.WriteAllText(fileName, messageText);

            Assert.IsTrue(_application.Utilities.ImportMessageFromFile(fileName, account.ID));
            Assert.IsFalse(_application.Utilities.ImportMessageFromFile(fileName, account.ID));

            POP3Simulator.AssertMessageCount("test@test.com", "test", 1);
        }

        [Test]
        [Description("Let the importer replace the full path in the database with a partial path")]
        public void TestReplaceFullPathWithPartialPath()
        {
            hMailServer.Account account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test");
            SMTPSimulator.StaticSend(account.Address, account.Address, "Test message", "Test body");

            hMailServer.IMAPFolder folder = account.IMAPFolders.get_ItemByName("Inbox");
            Utilities.AssertMessageExistsInFolder(folder, 1);

            hMailServer.Message message = account.IMAPFolders.get_ItemByName("Inbox").Messages[0];

            // Now nothing should happen.
            Assert.IsFalse(_application.Utilities.ImportMessageFromFile(message.Filename, account.ID));

            // Move the message file to another folder.
            string domainPath = Path.Combine(_application.Settings.Directories.DataDirectory, _domain.Name);
            string accountPath = Path.Combine(domainPath, "test");
            string fileName = Path.Combine(accountPath, "randomMail.eml");
            File.Move(message.Filename, fileName);

            // Update the database with the 'invalid' path.
            string sql = string.Format("update hm_messages set messagefilename = '{0}' where messageid = {1}", Utilities.Escape(fileName), message.ID);
            SingletonProvider<Utilities>.Instance.GetApp().Database.ExecuteSQL(sql);

            Assert.IsTrue(File.Exists(fileName));
            // Now the file should be moved to the correct path.
            Assert.IsTrue(_application.Utilities.ImportMessageFromFile(fileName, account.ID));

            Assert.IsFalse(File.Exists(fileName));

            // Now nothing should happen because the file is no longer there.
            Assert.IsFalse(_application.Utilities.ImportMessageFromFile(fileName, account.ID));

            string content = POP3Simulator.AssertGetFirstMessageText(account.Address, "test");

            Assert.IsTrue(content.Contains("Test message"));
        }

        [Test]
        [Description("Let the importer replace the full path in the database with a partial path")]
        public void TestReplaceInvalidPathWithCorrectPath()
        {
            hMailServer.Account account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test");
            SMTPSimulator.StaticSend(account.Address, account.Address, "Test message", "Test body");

            hMailServer.IMAPFolder folder = account.IMAPFolders.get_ItemByName("Inbox");
            Utilities.AssertMessageExistsInFolder(folder, 1);

            hMailServer.Message message = account.IMAPFolders.get_ItemByName("Inbox").Messages[0];

            // Now nothing should happen.
            Assert.IsFalse(_application.Utilities.ImportMessageFromFile(message.Filename, account.ID));


            string sql = string.Format("update hm_messages set messagefilename = '{0}' where messageid = {1}", Utilities.Escape(message.Filename), message.ID);

            SingletonProvider<Utilities>.Instance.GetApp().Database.ExecuteSQL(sql);

            // Now the path should be replaced.
            Assert.IsTrue(_application.Utilities.ImportMessageFromFile(message.Filename, account.ID));

            // Now nothing should happen.
            Assert.IsFalse(_application.Utilities.ImportMessageFromFile(message.Filename, account.ID));

            string content = POP3Simulator.AssertGetFirstMessageText(account.Address, "test");

            Assert.IsTrue(content.Contains("Test message"));
        }

        [Test]
        [Description("Let the importer replace the full path in the database with a partial path")]
        public void TestReplaceFullPathInPublicFolderWithPartialPath()
        {
            hMailServer.Application application = SingletonProvider<Utilities>.Instance.GetApp();
            hMailServer.Account account1 = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "account8@test.com", "test");

            hMailServer.IMAPFolders publicFolders = _settings.PublicFolders;
            hMailServer.IMAPFolder folder = publicFolders.Add("Share1");
            folder.Save();

            hMailServer.Message message = folder.Messages.Add();
            message.Subject = "Test";
            message.Save();

            // Move the message file to another folder.
            string publicFolderPath = Path.Combine(_application.Settings.Directories.DataDirectory, "#Public");
            string fileName = Path.Combine(publicFolderPath, "randomMail.eml");
            File.Move(message.Filename, fileName);

            // Update the database with the 'invalid' path.
            string sql = string.Format("update hm_messages set messagefilename = '{0}' where messageid = {1}", Utilities.Escape(fileName), message.ID);
            SingletonProvider<Utilities>.Instance.GetApp().Database.ExecuteSQL(sql);

            // Now try to insert the message.
            Assert.IsTrue(_application.Utilities.ImportMessageFromFile(fileName, 0));

            _application.Reinitialize();

            string newMessgaeFilename = _settings.PublicFolders[0].Messages[0].Filename;
            Assert.AreNotEqual(fileName, newMessgaeFilename);
            Assert.IsTrue(File.Exists(newMessgaeFilename));
        }

    }
}
