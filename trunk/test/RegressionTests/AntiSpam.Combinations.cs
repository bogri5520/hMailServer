// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Threading;
using System.Diagnostics;

namespace UnitTest.AntiSpam
{
   [TestFixture]
   public class Combinations : TestFixtureBase
   {
      [SetUp]
      public new void SetUp()
      {
         Utilities.AssertSpamAssassinIsRunning();
      }

      [Test]
      public void TestSpamMultipleHits()
      {
         hMailServer.Account oAccount1 = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "mult'ihit@test.com", "test");

         _settings.AntiSpam.SpamMarkThreshold = 1;
         _settings.AntiSpam.SpamDeleteThreshold = 2;

         _settings.AntiSpam.AddHeaderReason = true;
         _settings.AntiSpam.AddHeaderSpam = true;
         _settings.AntiSpam.PrependSubject = true;
         _settings.AntiSpam.PrependSubjectText = "ThisIsSpam";

         // Enable SpamAssassin
         _settings.AntiSpam.SpamAssassinEnabled = true;
         _settings.AntiSpam.SpamAssassinHost = "localhost";
         _settings.AntiSpam.SpamAssassinPort = 783;
         _settings.AntiSpam.SpamAssassinMergeScore = false;
         _settings.AntiSpam.SpamAssassinScore = 5;


         // Enable SURBL.
         hMailServer.SURBLServer oSURBLServer = _settings.AntiSpam.SURBLServers[0];
         oSURBLServer.Active = true;
         oSURBLServer.Score = 5;
         oSURBLServer.Save();

         // Send a messages to this account, containing both incorrect MX records an SURBL-hits.
         // We should only detect one of these two:
         SMTPClientSimulator oSMTP = new SMTPClientSimulator();

         _settings.Logging.LogSMTP = true;
         _settings.Logging.LogDebug = true;
         _settings.Logging.Enabled = true;
         _settings.Logging.EnableLiveLogging(true);

         // Access the log once to make sure it's cleared.
         string liveLog = _settings.Logging.LiveLog;

         // Should not be possible to send this email since it's results in a spam
         // score over the delete threshold.
         Assert.IsFalse(oSMTP.Send("test@domain_without_mx_records421dfsam430sasd.com", oAccount1.Address, "INBOX", "This is a test message. It contains incorrect MX records and a SURBL string: http://surbl-org-permanent-test-point.com/ SpamAssassinString: XJS*C4JDBQADN1.NSBN3*2IDNEN*GTUBE-STANDARD-ANTI-UBE-TEST-EMAIL*C.34X"));

         liveLog = _settings.Logging.LiveLog;

         _settings.Logging.EnableLiveLogging(false);

         int iFirst = liveLog.IndexOf("Running spam test");
         int iLast = liveLog.LastIndexOf("Running spam test");
         Assert.AreNotSame(-1, iFirst);

         // there should only be one spam test, since any spam match
         // should result in a spam score over the delete threshold.
         Assert.AreEqual(iFirst, iLast);
      }

      [Test]
      [Description("Test that only one result header is added if one test passes and one fails.")]
      public void TestOneFailOnePass()
      {
         hMailServer.Account oAccount1 = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "multihit@test.com", "test");

         _settings.AntiSpam.SpamMarkThreshold = 1;
         _settings.AntiSpam.SpamDeleteThreshold = 100;

         _settings.AntiSpam.AddHeaderReason = true;
         _settings.AntiSpam.AddHeaderSpam = true;
         _settings.AntiSpam.PrependSubject = true;
         _settings.AntiSpam.PrependSubjectText = "ThisIsSpam";

         _settings.AntiSpam.CheckHostInHelo = true;
         _settings.AntiSpam.CheckHostInHeloScore = 5;

         // Enable SURBL.
         hMailServer.SURBLServer oSURBLServer = _settings.AntiSpam.SURBLServers[0];
         oSURBLServer.Active = true;
         oSURBLServer.Score = 5;
         oSURBLServer.Save();

         // Send a messages to this account, containing both incorrect MX records an SURBL-hits.
         // We should only detect one of these two:
         SMTPClientSimulator oSMTP = new SMTPClientSimulator();

         // Should not be possible to send this email since it's results in a spam
         // score over the delete threshold.
         Assert.IsTrue(oSMTP.Send("test@domain_without_mx_records421dfsam430sasd.com", oAccount1.Address, "INBOX", "This is a test message."));

         string message = POP3Simulator.AssertGetFirstMessageText(oAccount1.Address, "test");

         Assert.IsTrue(message.Contains("X-hMailServer-Reason-1:"));
         Assert.IsFalse(message.Contains("X-hMailServer-Reason-2:"));
      }

      [Test]
      [Description("Confirm that if you have a delete threshold lower than the mark threshhold, spam tests are run until" + 
                   "the mark threshold is reached.")]
      public void TestDeleteThresholdLowerThanMarkThreshold()
      {
         hMailServer.Account oAccount1 = SingletonProvider<Utilities>.Instance.AddAccount(_domain, "multihit@test.com", "test");

         hMailServer.AntiSpam antiSpam = _settings.AntiSpam;

         antiSpam.SpamMarkThreshold = 15;
         antiSpam.SpamDeleteThreshold = 0;

         antiSpam.AddHeaderReason = true;
         antiSpam.AddHeaderSpam = true;
         antiSpam.PrependSubject = true;
         antiSpam.PrependSubjectText = "ThisIsSpam";

         antiSpam.CheckHostInHelo = true;
         antiSpam.CheckHostInHeloScore = 10;

         // Enable SURBL.
         hMailServer.SURBLServer oSURBLServer = antiSpam.SURBLServers[0];
         oSURBLServer.Active = true;
         oSURBLServer.Score = 10;
         oSURBLServer.Save();

         // Send a messages to this account, containing both incorrect MX records an SURBL-hits.
         // We should only detect one of these two:
         SMTPClientSimulator oSMTP = new SMTPClientSimulator();

         // Should not be possible to send this email since it's results in a spam
         // score over the delete threshold.
         Assert.IsTrue(oSMTP.Send("test@example.com", oAccount1.Address, "INBOX", "Test http://surbl-org-permanent-test-point.com/ Test 2"));

         string message = POP3Simulator.AssertGetFirstMessageText(oAccount1.Address, "test");

         Assert.IsTrue(message.Contains("X-hMailServer-Reason-1:"));
         Assert.IsTrue(message.Contains("X-hMailServer-Reason-2:"));
      }
   }
}
