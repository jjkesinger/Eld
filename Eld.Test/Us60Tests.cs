using Eld.Audit;
using Eld.Audit.Model;
using Eld.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Eld.Test
{
    [TestClass]
    public class Us60Tests
    {
        [TestMethod]
        public void Us60ShouldCorrectlyCalculateWeeklyViolationWith1stEventOffDutyNotReset()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("9/30/2018 23:00"), DateTime.Parse("10/1/2018 00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:00:00"), DateTime.Parse("10/1/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 17:00:00"), DateTime.Parse("10/2/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 07:00:00"), DateTime.Parse("10/2/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //20
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 17:00:00"), DateTime.Parse("10/3/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/3/2018 07:00:00"), DateTime.Parse("10/3/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //30
                new DutyStatusEvent("1", DateTime.Parse("10/3/2018 17:00:00"), DateTime.Parse("10/4/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/4/2018 07:00:00"), DateTime.Parse("10/4/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //40
                new DutyStatusEvent("1", DateTime.Parse("10/4/2018 17:00:00"), DateTime.Parse("10/5/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/5/2018 07:00:00"), DateTime.Parse("10/5/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //50
                new DutyStatusEvent("1", DateTime.Parse("10/5/2018 17:00:00"), DateTime.Parse("10/6/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/6/2018 07:00:00"), DateTime.Parse("10/6/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //60
                new DutyStatusEvent("1", DateTime.Parse("10/6/2018 17:00:00"), DateTime.Parse("10/7/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/7/2018 07:00:00"), DateTime.Parse("10/7/2018 19:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //72 <<- violation at 10/7 17:00:00
                new DutyStatusEvent("1", DateTime.Parse("10/7/2018 19:00:00"), DateTime.Parse("10/8/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/8/2018 07:00:00"), DateTime.Parse("10/8/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //82 <<- violation at 10/8 07:00:00
                new DutyStatusEvent("1", DateTime.Parse("10/8/2018 17:00:00"), DateTime.Parse("10/9/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
            };

            var driverViolationTable = new LogAuditor().Audit(logs);

            var weekly = new DutyStatusViolation(DateTime.Parse("10/7/2018 07:00:00"), DateTime.Parse("10/7/2018 19:00:00"), Enums.ViolationType.Weekly, Enums.Jurisdiction.Us60);
            var weekly2 = new DutyStatusViolation(DateTime.Parse("10/8/2018 07:00:00"), DateTime.Parse("10/8/2018 17:00:00"), Enums.ViolationType.Weekly, Enums.Jurisdiction.Us60);
            Assert.IsTrue(driverViolationTable["1"].First(f => f.ViolationType == Enums.ViolationType.Weekly).Equals(weekly));
            Assert.IsTrue(driverViolationTable["1"].Last(f => f.ViolationType == Enums.ViolationType.Weekly).Equals(weekly2));
        }

        [TestMethod]
        public void Us60ShouldGetOnDutyDrivingRestAndWeeklyViolation()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:00:00"), DateTime.Parse("10/11/2018 21:01:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60)
            };

            var driverViolationTable = new LogAuditor().Audit(logs);

            var restViolation = new DutyStatusViolation(DateTime.Parse("10/1/2018 15:00:00"), DateTime.Parse("10/11/2018 21:01:00"), Enums.ViolationType.Rest, Enums.Jurisdiction.Us60);
            var onDutyViolation = new DutyStatusViolation(DateTime.Parse("10/1/2018 21:00:00"), DateTime.Parse("10/11/2018 21:01:00"), Enums.ViolationType.OnDuty, Enums.Jurisdiction.Us60);
            var weekly = new DutyStatusViolation(DateTime.Parse("10/3/2018 19:00:00"), DateTime.Parse("10/11/2018 21:01:00"), Enums.ViolationType.Weekly, Enums.Jurisdiction.Us60);
            var driving = new DutyStatusViolation(DateTime.Parse("10/1/2018 18:00:00"), DateTime.Parse("10/11/2018 21:01:00"), Enums.ViolationType.Driving, Enums.Jurisdiction.Us60);

            Assert.IsTrue(driverViolationTable["1"].First(f => f.ViolationType == Enums.ViolationType.Rest).Equals(restViolation));
            Assert.IsTrue(driverViolationTable["1"].First(f => f.ViolationType == Enums.ViolationType.OnDuty).Equals(onDutyViolation));
            Assert.IsTrue(driverViolationTable["1"].FirstOrDefault(f => f.ViolationType == Enums.ViolationType.Weekly).Equals(weekly));
            Assert.IsTrue(driverViolationTable["1"].First(f => f.ViolationType == Enums.ViolationType.Driving).Equals(driving));
        }

        [TestMethod]
        public void Us60ShouldCorrectlyCalculateWeeklyViolations()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:00:00"), DateTime.Parse("10/1/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 17:00:00"), DateTime.Parse("10/2/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 07:00:00"), DateTime.Parse("10/2/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 17:00:00"), DateTime.Parse("10/3/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/3/2018 07:00:00"), DateTime.Parse("10/3/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/3/2018 17:00:00"), DateTime.Parse("10/4/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/4/2018 07:00:00"), DateTime.Parse("10/4/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/4/2018 17:00:00"), DateTime.Parse("10/5/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/5/2018 07:00:00"), DateTime.Parse("10/5/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/5/2018 17:00:00"), DateTime.Parse("10/6/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/6/2018 07:00:00"), DateTime.Parse("10/6/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/6/2018 17:00:00"), DateTime.Parse("10/7/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/7/2018 07:00:00"), DateTime.Parse("10/7/2018 19:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //12
                new DutyStatusEvent("1", DateTime.Parse("10/7/2018 19:00:00"), DateTime.Parse("10/8/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/8/2018 07:00:00"), DateTime.Parse("10/8/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/8/2018 17:00:00"), DateTime.Parse("10/9/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
            };

            var driverViolationTable = new LogAuditor().Audit(logs);

            var weekly = new DutyStatusViolation(DateTime.Parse("10/7/2018 07:00:00"), DateTime.Parse("10/7/2018 19:00:00"), Enums.ViolationType.Weekly, Enums.Jurisdiction.Us60);
            var weekly2 = new DutyStatusViolation(DateTime.Parse("10/8/2018 07:00:00"), DateTime.Parse("10/8/2018 17:00:00"), Enums.ViolationType.Weekly, Enums.Jurisdiction.Us60);
            Assert.IsTrue(driverViolationTable["1"].First(f => f.ViolationType == Enums.ViolationType.Weekly).Equals(weekly));
            Assert.IsTrue(driverViolationTable["1"].Last(f => f.ViolationType == Enums.ViolationType.Weekly).Equals(weekly2));
        }

        [TestMethod]
        public void Us60ShouldCorrectlyCalculateWeeklyViolationsRollingDays()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 01:00:00"), DateTime.Parse("10/1/2018 18:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //17
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 18:00:00"), DateTime.Parse("10/2/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 07:00:00"), DateTime.Parse("10/2/2018 12:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //5
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 12:00:00"), DateTime.Parse("10/3/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/3/2018 07:00:00"), DateTime.Parse("10/3/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/3/2018 17:00:00"), DateTime.Parse("10/4/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/4/2018 07:00:00"), DateTime.Parse("10/4/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/4/2018 17:00:00"), DateTime.Parse("10/5/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/5/2018 07:00:00"), DateTime.Parse("10/5/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/5/2018 17:00:00"), DateTime.Parse("10/6/2018 13:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/6/2018 13:00:00"), DateTime.Parse("10/6/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //4
                new DutyStatusEvent("1", DateTime.Parse("10/6/2018 17:00:00"), DateTime.Parse("10/7/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/7/2018 09:00:00"), DateTime.Parse("10/7/2018 13:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //4
                new DutyStatusEvent("1", DateTime.Parse("10/7/2018 13:00:00"), DateTime.Parse("10/8/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/8/2018 03:00:00"), DateTime.Parse("10/8/2018 20:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //17
                new DutyStatusEvent("1", DateTime.Parse("10/8/2018 20:00:00"), DateTime.Parse("10/9/2018 04:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/9/2018 04:00:00"), DateTime.Parse("10/9/2018 09:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //5
                new DutyStatusEvent("1", DateTime.Parse("10/9/2018 09:00:00"), DateTime.Parse("10/10/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
            };

            var driverViolationTable = new LogAuditor().Audit(logs);

            Assert.IsTrue(driverViolationTable["1"].FirstOrDefault(f => f.ViolationType == Enums.ViolationType.Weekly) == null);
        }

        [TestMethod]
        public void Us60ShouldCorrectlyCalculateWeeklyViolationsRollingDaysWithViolation()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 01:00:00"), DateTime.Parse("10/1/2018 18:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //17
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 18:00:00"), DateTime.Parse("10/2/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 07:00:00"), DateTime.Parse("10/2/2018 12:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //5
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 12:00:00"), DateTime.Parse("10/3/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/3/2018 07:00:00"), DateTime.Parse("10/3/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/3/2018 17:00:00"), DateTime.Parse("10/4/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/4/2018 07:00:00"), DateTime.Parse("10/4/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/4/2018 17:00:00"), DateTime.Parse("10/5/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/5/2018 07:00:00"), DateTime.Parse("10/5/2018 17:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //10
                new DutyStatusEvent("1", DateTime.Parse("10/5/2018 17:00:00"), DateTime.Parse("10/6/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/6/2018 07:00:00"), DateTime.Parse("10/6/2018 14:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //7
                new DutyStatusEvent("1", DateTime.Parse("10/6/2018 14:00:00"), DateTime.Parse("10/7/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

                new DutyStatusEvent("1", DateTime.Parse("10/7/2018 00:00:00"), DateTime.Parse("10/8/2018 07:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60), //0

                new DutyStatusEvent("1", DateTime.Parse("10/8/2018 04:00:00"), DateTime.Parse("10/8/2018 23:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //18
                new DutyStatusEvent("1", DateTime.Parse("10/8/2018 23:00:00"), DateTime.Parse("10/9/2018 04:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
            };

            var driverViolationTable = new LogAuditor().Audit(logs);
            var weekly = new DutyStatusViolation(DateTime.Parse("10/8/2018 22:00:00"), DateTime.Parse("10/8/2018 23:00:00"), Enums.ViolationType.Weekly, Enums.Jurisdiction.Us60);
            Assert.IsTrue(driverViolationTable["1"].First(f => f.ViolationType == Enums.ViolationType.Weekly).Equals(weekly));
        }

        [TestMethod]
        public void Us60RollingWeeklyTest()
        {
            Stopwatch sw = new Stopwatch();
            DateTimeOffset now = DateTimeOffset.Now;
            int driverCount = 100;

            var logs = new List<DutyStatusEvent>();
            for (var i = 0; i < driverCount; i++)
            {
                for (var j = 176; j >= 0; j--)
                {
                    logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 0, 0, 0), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 7, 0, 0), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60));
                    logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 7, 0, 0), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 15, 0, 0), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60));
                    logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 15, 0, 0), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 15, 30, 0), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60));
                    logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 15, 30, 0), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 18, 30, 0), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60));
                    logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 18, 30, 0), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 21, 00, 0), Enums.DutyStatus.OnDutyNotDriving, Enums.Jurisdiction.Us60));
                    logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 21, 00, 0), new DateTime(now.AddDays(-j + 1).Year, now.AddDays(-j + 1).Month, now.AddDays(-j + 1).Day, 0, 0, 0), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60));
                }
            }

            sw.Start();
            var driverViolationTable = new LogAuditor().Audit(logs);
            sw.Stop();

            for (var i = 0; i < driverCount; i++)
            {
                int weeklyCount = driverViolationTable[i.ToString()].Count(f => f.ViolationType == Enums.ViolationType.Weekly);
                Assert.IsTrue((logs.Max(f => f.EndDateTime.Date).Subtract(logs.Min(f => f.StartDateTime.Date)).TotalDays - 5 + 1) * 2 == weeklyCount); //-6 days before we get our first violation +1 to include last log date
            }
            Assert.IsTrue(sw.Elapsed < TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public void Us60Rolling70WeeklyTestNoViolations()
        {
            Stopwatch sw = new Stopwatch();

            int driverCount = 100;
            DateTimeOffset now = DateTimeOffset.Now;
            var logs = new List<DutyStatusEvent>();
            for (var i = 0; i < driverCount; i++)
            {
                for (var j = 180; j >= 0; j--)
                {
                    if (j % 5 == 0)
                    {
                        logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 0, 0, 0), new DateTime(now.AddDays(-j + 2).Year, now.AddDays(-j + 2).Month, now.AddDays(-j + 2).Day, 0, 0, 0), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60));
                        j--;
                    }
                    else
                    {
                        logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 0, 0, 0), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 7, 0, 0), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60));
                        logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 7, 0, 0), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 15, 0, 0), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60));
                        logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 15, 0, 0), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 15, 30, 0), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60));
                        logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 15, 30, 0), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 18, 30, 0), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60));
                        logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 18, 30, 0), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 21, 00, 0), Enums.DutyStatus.OnDutyNotDriving, Enums.Jurisdiction.Us60));
                        logs.Add(new DutyStatusEvent(i.ToString(), new DateTime(now.AddDays(-j).Year, now.AddDays(-j).Month, now.AddDays(-j).Day, 21, 00, 0), new DateTime(now.AddDays(-j + 1).Year, now.AddDays(-j + 1).Month, now.AddDays(-j + 1).Day, 0, 0, 0), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60));
                    }
                }

            }

            sw.Start();
            var driverViolationTable = new LogAuditor().Audit(logs);
            sw.Stop();

            Assert.IsTrue(driverViolationTable.Count() == 0);
            Assert.IsTrue(sw.Elapsed < TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public void Us60ShouldCorrectlyIdentifySplitSleeper()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 00:00:00"), DateTime.Parse("10/1/2018 08:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //drive 8
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 08:00:00"), DateTime.Parse("10/1/2018 16:00:00"), Enums.DutyStatus.Sleeper, Enums.Jurisdiction.Us60), //sleeper 8
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 16:00:00"), DateTime.Parse("10/1/2018 19:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //drive 3
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 19:00:00"), DateTime.Parse("10/1/2018 22:00:00"), Enums.DutyStatus.OnDutyNotDriving, Enums.Jurisdiction.Us60), //onduty 3
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 22:00:00"), DateTime.Parse("10/2/2018 00:00:00"), Enums.DutyStatus.Sleeper, Enums.Jurisdiction.Us60), //sleeper 2
                
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 00:00:00"), DateTime.Parse("10/2/2018 08:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //drive 8
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 08:00:00"), DateTime.Parse("10/2/2018 16:00:00"), Enums.DutyStatus.Sleeper, Enums.Jurisdiction.Us60), //sleeper 8
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 16:00:00"), DateTime.Parse("10/2/2018 19:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //drive 3
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 19:00:00"), DateTime.Parse("10/2/2018 22:00:00"), Enums.DutyStatus.OnDutyNotDriving, Enums.Jurisdiction.Us60), //onduty 3
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 22:00:00"), DateTime.Parse("10/3/2018 00:00:00"), Enums.DutyStatus.Sleeper, Enums.Jurisdiction.Us60), //sleeper 2
            };

            var driverViolationTable = new LogAuditor().Audit(logs);
            Assert.IsTrue(driverViolationTable.Count() == 0);
        }

        [TestMethod]
        public void Us60ShouldCorrectlyIdentifyViolationAfterSplitSleeper()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 00:00:00"), DateTime.Parse("10/1/2018 08:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //drive 8
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 08:00:00"), DateTime.Parse("10/1/2018 16:00:00"), Enums.DutyStatus.Sleeper, Enums.Jurisdiction.Us60), //sleeper 8
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 16:00:00"), DateTime.Parse("10/1/2018 19:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //drive 3
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 19:00:00"), DateTime.Parse("10/1/2018 22:00:00"), Enums.DutyStatus.OnDutyNotDriving, Enums.Jurisdiction.Us60), //onduty 3
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 22:00:00"), DateTime.Parse("10/2/2018 00:00:00"), Enums.DutyStatus.Sleeper, Enums.Jurisdiction.Us60), //sleeper 2
                
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 00:00:00"), DateTime.Parse("10/2/2018 08:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //drive 8
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 08:00:00"), DateTime.Parse("10/2/2018 16:00:00"), Enums.DutyStatus.Sleeper, Enums.Jurisdiction.Us60), //sleeper 8
                new DutyStatusEvent("1", DateTime.Parse("10/2/2018 16:00:00"), DateTime.Parse("10/2/2018 19:00:00"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60), //drive 3 <<since no reset this is in violation
            };

            var driverViolationTable = new LogAuditor().Audit(logs);
            DutyStatusViolation daily = new DutyStatusViolation(DateTime.Parse("10/2/2018 16:00:00"), DateTime.Parse("10/2/2018 19:00:00"), Enums.ViolationType.OnDuty, Enums.Jurisdiction.Us60);
            Assert.IsTrue(driverViolationTable["1"].First(f => f.ViolationType == Enums.ViolationType.OnDuty).Equals(daily));
        }

        [TestMethod]
        public void Us60ShouldIdentifyOnDutyViolation()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 00:00:00"), DateTime.Parse("10/1/2018 07:05:20"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:05:20"), DateTime.Parse("10/1/2018 07:16:11"), Enums.DutyStatus.OnDutyNotDriving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:16:11"), DateTime.Parse("10/1/2018 12:14:22"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 12:14:22"), DateTime.Parse("10/1/2018 12:45:02"), Enums.DutyStatus.Sleeper, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 12:45:02"), DateTime.Parse("10/1/2018 17:50:54"), Enums.DutyStatus.OnDutyNotDriving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 17:50:54"), DateTime.Parse("10/1/2018 19:05:55"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 19:05:55"), DateTime.Parse("10/1/2018 21:59:41"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 21:59:41"), DateTime.Parse("10/2/2018 00:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),

            };

            var driverViolationTable = new LogAuditor().Audit(logs);
            DutyStatusViolation ondutyViolation = new DutyStatusViolation(DateTime.Parse("10/1/2018 21:05:20"), DateTime.Parse("10/1/2018 21:59:41"), Enums.ViolationType.OnDuty, Enums.Jurisdiction.Us60);

            Assert.IsTrue(driverViolationTable["1"].Count() == 1);
            Assert.IsTrue(driverViolationTable["1"].First(g => g.ViolationType == Enums.ViolationType.OnDuty).Equals(ondutyViolation));
        }

        [TestMethod]
        public void Us60ShouldIdentifyOnDutyViolationWithoutLeadingAndTrailingOff()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:05:20"), DateTime.Parse("10/1/2018 07:16:11"), Enums.DutyStatus.OnDutyNotDriving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:16:11"), DateTime.Parse("10/1/2018 12:14:22"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 12:14:22"), DateTime.Parse("10/1/2018 12:45:02"), Enums.DutyStatus.Sleeper, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 12:45:02"), DateTime.Parse("10/1/2018 17:50:54"), Enums.DutyStatus.OnDutyNotDriving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 17:50:54"), DateTime.Parse("10/1/2018 19:05:55"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 19:05:55"), DateTime.Parse("10/1/2018 21:59:41"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60),

            };

            var driverViolationTable = new LogAuditor().Audit(logs);
            DutyStatusViolation ondutyViolation = new DutyStatusViolation(DateTime.Parse("10/1/2018 21:05:20"), DateTime.Parse("10/1/2018 21:59:41"), Enums.ViolationType.OnDuty, Enums.Jurisdiction.Us60);

            Assert.IsTrue(driverViolationTable["1"].Count() == 1);
            Assert.IsTrue(driverViolationTable["1"].First(g => g.ViolationType == Enums.ViolationType.OnDuty).Equals(ondutyViolation));
        }

        [TestMethod]
        public void Us60ShouldIdentifyDrivingViolation()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 00:00:00"), DateTime.Parse("10/1/2018 07:05:20"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:05:20"), DateTime.Parse("10/1/2018 14:05:20"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 14:05:20"), DateTime.Parse("10/1/2018 14:37:32"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 14:37:32"), DateTime.Parse("10/1/2018 19:14:32"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 19:14:32"), DateTime.Parse("10/2/2018 00:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
            };

            var driverViolationTable = new LogAuditor().Audit(logs);
            DutyStatusViolation violation = new DutyStatusViolation(DateTime.Parse("10/1/2018 18:37:32"), DateTime.Parse("10/1/2018 19:14:32"), Enums.ViolationType.Driving, Enums.Jurisdiction.Us60);

            Assert.IsTrue(driverViolationTable["1"].Count() == 1);
            Assert.IsTrue(driverViolationTable["1"].First(g => g.ViolationType == Enums.ViolationType.Driving).Equals(violation));
        }

        [TestMethod]
        public void Us60ShouldIdentifyDrivingViolationWithoutLeadingAndTrailingOff()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:05:20"), DateTime.Parse("10/1/2018 14:05:20"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 14:05:20"), DateTime.Parse("10/1/2018 14:37:32"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 14:37:32"), DateTime.Parse("10/1/2018 19:14:32"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60),
            };

            var driverViolationTable = new LogAuditor().Audit(logs);
            DutyStatusViolation violation = new DutyStatusViolation(DateTime.Parse("10/1/2018 18:37:32"), DateTime.Parse("10/1/2018 19:14:32"), Enums.ViolationType.Driving, Enums.Jurisdiction.Us60);

            Assert.IsTrue(driverViolationTable["1"].Count() == 1);
            Assert.IsTrue(driverViolationTable["1"].First(g => g.ViolationType == Enums.ViolationType.Driving).Equals(violation));
        }

        [TestMethod]
        public void Us60ShouldIdentifyRestViolation()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 00:00:00"), DateTime.Parse("10/1/2018 07:05:20"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:05:20"), DateTime.Parse("10/1/2018 15:35:20"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60),
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 15:35:20"), DateTime.Parse("10/2/2018 00:00:00"), Enums.DutyStatus.OffDuty, Enums.Jurisdiction.Us60),
            };

            var driverViolationTable = new LogAuditor().Audit(logs);
            DutyStatusViolation violation = new DutyStatusViolation(DateTime.Parse("10/1/2018 15:05:20"), DateTime.Parse("10/1/2018 15:35:20"), Enums.ViolationType.Rest, Enums.Jurisdiction.Us60);

            Assert.IsTrue(driverViolationTable["1"].Count() == 1);
            Assert.IsTrue(driverViolationTable["1"].First(g => g.ViolationType == Enums.ViolationType.Rest).Equals(violation));
        }

        [TestMethod]
        public void Us60ShouldIdentifyRestViolationWithoutLeadingAndTrailingOff()
        {
            var logs = new List<DutyStatusEvent>
            {
                new DutyStatusEvent("1", DateTime.Parse("10/1/2018 07:05:20"), DateTime.Parse("10/1/2018 15:35:20"), Enums.DutyStatus.Driving, Enums.Jurisdiction.Us60),
            };

            var driverViolationTable = new LogAuditor().Audit(logs);
            DutyStatusViolation violation = new DutyStatusViolation(DateTime.Parse("10/1/2018 15:05:20"), DateTime.Parse("10/1/2018 15:35:20"), Enums.ViolationType.Rest, Enums.Jurisdiction.Us60);

            Assert.IsTrue(driverViolationTable["1"].Count() == 1);
            Assert.IsTrue(driverViolationTable["1"].First(g => g.ViolationType == Enums.ViolationType.Rest).Equals(violation));
        }
    }
}
