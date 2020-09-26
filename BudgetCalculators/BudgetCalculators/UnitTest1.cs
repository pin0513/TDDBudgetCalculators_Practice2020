using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetCalculators
{
    [TestClass]
    public class BudgetCalculatorTests
    {
        IBudgetRepo repo = getTestBudget();

        private static IBudgetRepo getTestBudget()
        {
            IBudgetRepo repo = new MockBudgetRepo(new List<Budget>()
            {
                new Budget("202001", 310),
                new Budget("202002", 2900),
                new Budget("202003", 310),
                new Budget("202004", 3000),
            });
            return repo;
        }

        [TestMethod]
        public void NoBudget()
        {
            BudgetService bc = new BudgetService(repo);
            var amount = bc.Query(new DateTime(2019, 1, 1), new DateTime(2019, 1, 1));
            Assert.AreEqual(amount, 0);
        }

        [TestMethod]
        public void OneMonthBudget()
        {
            BudgetService bc = new BudgetService(repo);
            var amount = bc.Query(new DateTime(2020, 1, 1), new DateTime(2020, 1, 31));
            Assert.AreEqual(amount, 310);
        }

        [TestMethod]
        public void OneDayBudget()
        {
            BudgetService bc = new BudgetService(repo);
            var amount = bc.Query(new DateTime(2020, 1, 1), new DateTime(2020, 1, 1));
            Assert.AreEqual(amount, 10);
        }

        [TestMethod]
        public void TwoDayInOneMonthBudget()
        {
            BudgetService bc = new BudgetService(repo);
            var amount = bc.Query(new DateTime(2020, 1, 1), new DateTime(2020, 1, 2));
            Assert.AreEqual(amount, 20);
        }        

        [TestMethod]
        public void TwoDayInOneMonthBudget2()
        {
            BudgetService bc = new BudgetService(repo);
            var amount = bc.Query(new DateTime(2020, 1, 2), new DateTime(2020, 1, 3));
            Assert.AreEqual(amount, 20);
        }  
        
        [TestMethod]
        public void TwoMonthBudget()
        {
            BudgetService bc = new BudgetService(repo);
            var amount = bc.Query(new DateTime(2020, 3, 1), new DateTime(2020, 4, 30));
            Assert.AreEqual(amount, 3310);
        }

        [TestMethod]
        public void ManyDayInTwoMonthBudget()
        {
            BudgetService bc = new BudgetService(repo);
            var amount = bc.Query(new DateTime(2020, 1, 31), new DateTime(2020, 2, 1));
            Assert.AreEqual(amount, 110);
        }

        [TestMethod]
        public void ManyDayInThreeMonthBudget()
        {
            BudgetService bc = new BudgetService(repo);
            var amount = bc.Query(new DateTime(2020, 1, 31), new DateTime(2020, 3, 1));
            Assert.AreEqual(amount, 2920);
        }


        
        [TestMethod]
        public void LargeSearchDatetime()
        {
            BudgetService bc = new BudgetService(repo);
            var amount = bc.Query(new DateTime(2019, 12, 31), new DateTime(2020, 5, 1));
            Assert.AreEqual(amount, 6520);
        }



        [TestMethod]
        public void NoValidStartEndDate()
        {
            BudgetService bc = new BudgetService(repo);
            var amount = bc.Query(new DateTime(2020, 1, 1), new DateTime(2019, 1, 1));
            Assert.AreEqual(amount, 0);
        }


    }

    public class MockBudgetRepo : IBudgetRepo
    {
        private List<Budget> _data;

        public MockBudgetRepo(List<Budget> data)
        {
            _data = data;
        }

        public List<Budget> getAll()
        {
            return _data;
        }
    }

    public interface IBudgetRepo
    {
        List<Budget> getAll();
    }

    public class Budget
    {
        public Budget(string yearMonth, int amount)
        {
            YearMonth = yearMonth;
            Amount = amount;
        }
        public string YearMonth;
        public int Amount;
    }

    public class BudgetService
    {
        public BudgetService(IBudgetRepo repo)
        {
            _repo = repo;
        }

        private IBudgetRepo _repo;

        public double Query(DateTime start, DateTime end)
        {
            if (end < start) return 0;

            var startYearMonth = Convert.ToInt32(start.ToString("yyyyMM"));
            var endYearMonth = Convert.ToInt32(end.ToString("yyyyMM"));
            var searchBudgetResult = _repo.getAll().Where(x => startYearMonth <= Convert.ToInt32(x.YearMonth) 
                                                   && Convert.ToInt32(x.YearMonth) <= endYearMonth).ToList();

            if (!searchBudgetResult.Any())
            {
                return 0;
            }
            else
            {
                var allAmount = 0;
                foreach (var monthBudget in searchBudgetResult)
                {
                    DateTime monthStart = start;
                    DateTime monthEnd = end;
                    var year = getBudgetYear(monthBudget);
                    var month = getBudgetMonth(monthBudget);

                    if (start.Year == end.Year && start.Month == end.Month)
                    {
                        monthStart = start;
                        monthEnd = end;
                    }
                    else
                    {
                        
                        if (monthBudget.YearMonth == startYearMonth.ToString() )
                        {
                            monthStart = start;
                            monthEnd = new DateTime(year, month, DateTime.DaysInMonth(start.Year, start.Month));
                        }

                        if (monthBudget.YearMonth == endYearMonth.ToString())
                        {
                            monthStart = new DateTime(year, month, 1);
                            monthEnd = end;
                        }
                        if (monthBudget.YearMonth != startYearMonth.ToString() &&
                            monthBudget.YearMonth != endYearMonth.ToString())
                        {
                            monthStart = new DateTime(year, month, 1);
                            monthEnd = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                        }
                    }

                    var daydiff = (monthEnd - monthStart).Days + 1;

                    allAmount += monthBudget.Amount / DateTime.DaysInMonth(monthStart.Year, monthStart.Month) * daydiff;

                }

                return allAmount;
            }

        }

        private static int getBudgetMonth(Budget monthBudget)
        {
            return Convert.ToInt32(monthBudget.YearMonth.Substring(4, 2));
        }

        private static int getBudgetYear(Budget monthBudget)
        {
            return Convert.ToInt32(monthBudget.YearMonth.Substring(0, 4));
        }
    }
}
