﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace UnitTest.Protocols.IMAP
{
    [TestFixture]
    public class Append : TestFixtureBase
    {
        [Test]
        public void TestAppend()
        {
            hMailServer.Account oAccount = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "check@test.com", "test");
            IMAPSimulator oSimulator = new IMAPSimulator();

            string sWelcomeMessage = oSimulator.Connect();
            oSimulator.LogonWithLiteral("check@test.com", "test");
            oSimulator.SendSingleCommandWithLiteral("A01 APPEND INBOX {4}", "ABCD");
            Assert.AreEqual(1, oSimulator.GetMessageCount("INBOX"));
            oSimulator.Disconnect();
        }

        [Test]
        [Description("Append a message to an account inbox and make sure it's placed in the right location on disk.")]
        public void ConfirmFileAddedToCorrectAccountFolder()
        {
            Utilities utilities = SingletonProvider<Utilities>.Instance;
            hMailServer.Account oAccount = utilities.AddAccount(_domain, "check@test.com", "test");
            IMAPSimulator oSimulator = new IMAPSimulator();

            // Confirm that the public folder is empty before we start our test.
            string publicDir = utilities.GetPublicDirectory();
            utilities.AssertFilesInDirectory(publicDir, 0);

            // Add a message to the inbox.
            oSimulator.Connect();
            oSimulator.LogonWithLiteral("check@test.com", "test");
            oSimulator.SendSingleCommandWithLiteral("A01 APPEND INBOX {4}", "ABCD");
            
            // Confirm it exists in the IMAP folder.
            Assert.AreEqual(1, oSimulator.GetMessageCount("INBOX"));
            oSimulator.Disconnect();
            
            // The public directory should still be empty - the message was added to the user account.
            utilities.AssertFilesInDirectory(publicDir, 0);

            // There should be a single file in the users directory.
            utilities.AssertFilesInUserDirectory(oAccount, 1);

                
        }

        [Test]
        [Description("Append a message to an public folder and make sure it's placed in the right location on disk.")]
        public void ConfirmFileAddedToCorrectPublicFolder()
        {
            Utilities utilities = SingletonProvider<Utilities>.Instance;
            hMailServer.Account oAccount = utilities.AddAccount(_domain, "check@test.com", "test");
            IMAPSimulator oSimulator = new IMAPSimulator();

            // Confirm that the public folder is empty before we start our test.
            string publicDir = utilities.GetPublicDirectory();
            utilities.AssertFilesInDirectory(publicDir, 0);

            hMailServer.IMAPFolders folders = _application.Settings.PublicFolders;
            hMailServer.IMAPFolder folder = folders.Add("Share");
            folder.Save();
         
            // Give everyone access to the folder.
            hMailServer.IMAPFolderPermission permission = folder.Permissions.Add();
            permission.PermissionType = hMailServer.eACLPermissionType.ePermissionTypeAnyone;
            permission.set_Permission(hMailServer.eACLPermission.ePermissionLookup, true);
            permission.set_Permission(hMailServer.eACLPermission.ePermissionRead, true);
            permission.set_Permission(hMailServer.eACLPermission.ePermissionInsert, true);
            permission.Save();
            
            // Add the message to the public folder.
            oSimulator.Connect();
            oSimulator.LogonWithLiteral("check@test.com", "test");
            oSimulator.SendSingleCommandWithLiteral("A01 APPEND #Public.Share {4}", "ABCD");

            // Confirm that the message exists in the public folder and not in the inbox.
            Assert.AreEqual(1, oSimulator.GetMessageCount("#Public.Share"));
            Assert.AreEqual(0, oSimulator.GetMessageCount("INBOX"));
            oSimulator.Disconnect();


            // The public directory should now contain the message.
            utilities.AssertFilesInDirectory(publicDir, 1);

            // There users directory should still be empty.
            utilities.AssertFilesInUserDirectory(oAccount, 0);

                
        }

        [Test]
        public void TestDomainMaxMessageSizeLimitEnabled()
        {
           var account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test", 0);
           var message = new StringBuilder();
           
           // ~2 kb string
           for (int i = 0; i < 25; i++)
              message.AppendLine("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");

           _domain.MaxMessageSize = 1; // 1 kb
           _domain.Save();

           var imapSim = new IMAPSimulator("test@test.com", "test", "INBOX");
           string result = imapSim.SendSingleCommandWithLiteral("A01 APPEND INBOX {" + message.Length + "}", message.ToString());
           imapSim.Logout();

           Assert.IsTrue(result.StartsWith("A01 NO Message size exceeds fixed maximum message size."));
        }

        [Test]
        public void TestDomainMaxMessageSizeLimitDisabled()
        {
           var account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test", 0);
           var message = new StringBuilder();
           
           // ~2 kb string
           for (int i = 0; i < 25; i++)
              message.AppendLine("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");

           _domain.MaxMessageSize = 0; // 1 kb
           _domain.Save();

           var imapSim = new IMAPSimulator("test@test.com", "test", "INBOX");
           string result = imapSim.SendSingleCommandWithLiteral("A01 APPEND INBOX {" + message.Length + "}", message.ToString());
           imapSim.Logout();

           Assert.IsFalse(result.StartsWith("A01 NO Message size exceeds fixed maximum message size."));
        }

        [Test]
        public void TestGlobalMaxMessageSizeLimitEnabled()
        {
           var account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test", 0);
           var message = new StringBuilder();

           // ~2 kb string
           for (int i = 0; i < 25; i++)
              message.AppendLine("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");

           _settings.MaxMessageSize = 1;

           var imapSim = new IMAPSimulator("test@test.com", "test", "INBOX");
           string result = imapSim.SendSingleCommandWithLiteral("A01 APPEND INBOX {" + message.Length + "}", message.ToString());
           imapSim.Logout();

           Assert.IsTrue(result.StartsWith("A01 NO Message size exceeds fixed maximum message size."));
        }

        [Test]
        public void TestGlobalMaxMessageSizeLimitDisabled()
        {
           var account = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "test@test.com", "test", 0);
           var message = new StringBuilder();

           // ~2 kb string
           for (int i = 0; i < 25; i++)
              message.AppendLine("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");

           _settings.MaxMessageSize = 0;

           var imapSim = new IMAPSimulator("test@test.com", "test", "INBOX");
           string result = imapSim.SendSingleCommandWithLiteral("A01 APPEND INBOX {" + message.Length + "}", message.ToString());
           imapSim.Logout();

           Assert.IsFalse(result.StartsWith("A01 NO Message size exceeds fixed maximum message size."));
        }
    }
}

