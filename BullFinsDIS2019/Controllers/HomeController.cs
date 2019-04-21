﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BullFinsDIS2019.Models;
using static BullFinsDIS2019.Models.EF_Models;
// ADD THESE DIRECTIVES
using BullFinsDIS2019.Data;
using Newtonsoft.Json;
using System.Net.Http;

namespace BullFinsDIS2019.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /*
            These lines are needed to use the Database context,
            define the connection to the API, and use the
            HttpClient to request data from the API
        */

        public ApplicationDbContext dbContext;
        //Base URL for the IEXTrading API. Method specific URLs are appended to this base URL.
        string BASE_URL = "https://api.iextrading.com/1.0/";
        HttpClient httpClient;

        /*
             These lines create a Constructor for the HomeController.
             Then, the Database context is defined in a variable.
             Then, an instance of the HttpClient is created.
        */
        public HomeController(ApplicationDbContext context)
        {
            dbContext = context;

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new
                System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        /*
            Calls the IEX reference API to get the list of symbols.
            Returns a list of the companies whose information is available. 
        */
        public List<Company> GetSymbols()
        {
            string IEXTrading_API_PATH = BASE_URL + "ref-data/symbols";
            string companyList = "";
            List<Company> companies = null;

            // connect to the IEXTrading API and retrieve information
            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                companyList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            // now, parse the Json strings as C# objects
            if (!companyList.Equals(""))
            {
                // https://stackoverflow.com/a/46280739
                companies = JsonConvert.DeserializeObject<List<Company>>(companyList);
                companies = companies.GetRange(0, 50);
            }

            return companies;
        }

        /*
            Calls the IEX reference API to get the stock stats.
            Returns a stock stats whose information is available. 
        */
        public StockStats GetStockStats(String symbol)
        {
            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/stats";
            string responseStockStats = "";
            StockStats stockStats = null;

            HttpClient httpClient1 = new HttpClient();
            httpClient1.DefaultRequestHeaders.Accept.Clear();
            httpClient1.DefaultRequestHeaders.Accept.Add(new
                System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // connect to the IEXTrading API and retrieve information
            httpClient1.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient1.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                responseStockStats = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.WriteLine(responseStockStats);
            }

            // now, parse the Json strings as C# objects
            if (!responseStockStats.Equals(""))
            {
                stockStats = JsonConvert.DeserializeObject<StockStats>(responseStockStats);
            }

            return stockStats;
        }

        // Get User Portolio Information from the database
        public List<UserStocks> GetUserPortfolio(String user) {
            IQueryable<UserStocks> portfolio = dbContext.UserStock.Where(c => c.user.Equals(user));

            List<UserStocks> stockList = new List<UserStocks>();
            foreach (UserStocks stock in portfolio)
            {
                stockList.Add(stock);
            }
            return stockList;
        }


        // Get User Portolio Information from the database
        public void UpdateUserPortfolio(String user, String symbol, String quantity)
        {
            if (dbContext.UserStock.Where(c => c.user.Equals(user) && c.symbol.Equals(symbol)).Count() == 0)
            {
                UserStocks stock = new UserStocks();
                stock.user = user;
                stock.symbol = symbol;
                stock.quantity = quantity;
                dbContext.UserStock.Add(stock);
            } else {
                IQueryable<UserStocks> userStockList = dbContext.UserStock.Where(c => c.user.Equals(user) && c.symbol.Equals(symbol));
                String oldQuantity = userStockList.First().quantity;
                userStockList.First().quantity = (int.Parse(oldQuantity) + int.Parse(quantity)) + "";
            }
            dbContext.SaveChanges();
        }

        // Calling the charts API
        public List<Chart> GetChartData(String symbol)
        {
            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/chart/ytd";
            string responseChartData = "";
            List<Chart> chartData = null;

            // connect to the IEXTrading API and retrieve information
            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                responseChartData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.WriteLine(responseChartData);
            }

            // now, parse the Json strings as C# objects
            if (!responseChartData.Equals(""))
            {
                chartData = JsonConvert.DeserializeObject<List<Chart>>(responseChartData);
            }

            return chartData;
        }

        /*
           Calls the IEX reference API to get the financials stats.
           Returns Financials of the companies whose information is available. 
       */
        public SymbolFinancial GetFinancials(String symbol)
        {
            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/financials";
            string responseFinancials = "";
            SymbolFinancial symbolFinancial = null;

            // connect to the IEXTrading Financial API and retrieve information
            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // read the Json objects in the Financial API response
            if (response.IsSuccessStatusCode)
            {
                responseFinancials = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.WriteLine(responseFinancials);
            }

            // now, parse the Json strings as C# objects
            if (!responseFinancials.Equals(""))
            {
                //chartData = JsonConvert.DeserializeObject<List<Chart>>(responseChartData);
                try
                {
                    symbolFinancial = JsonConvert.DeserializeObject<SymbolFinancial>(responseFinancials);
                }
                catch (Exception ex)
                {
                    symbolFinancial = new SymbolFinancial();
                }
            }

            return (symbolFinancial);
        }

        /*
            The Symbols action calls the GetSymbols method that returns a list of Companies.
            This list of Companies is passed to the Symbols View.
        */
        public IActionResult Symbols()
        {
            List<Company> companies = GetSymbols();
            PopulateSymbols(companies);
            return View(companies);
        }

        public IActionResult Portfolio(String user)
        {
            return View(GetUserPortfolio(user));
        }


        public IActionResult BuyRequest(String symbol)
        {
            ViewBag.symbol = symbol;
            return View();
        }

        public IActionResult BuySubmit(String quantity, String symbol, String userName)
        {
            ViewBag.symbol   = symbol;
            ViewBag.quantity = quantity;
            ViewBag.user     = userName;

            UpdateUserPortfolio(userName, symbol, quantity);
            return View();
        }

        public IActionResult Financials(String symbol)
        {
            SymbolFinancial financials = GetFinancials(symbol);
            PopulateSymbolFinancialData(financials, symbol);

            List<Financials> financialList = financials.financials;
            return View(financialList);
        }

        public IActionResult ChartData(String symbol)
        {
            StockStats stockstats = GetStockStats(symbol);
            PopulateStockStats(stockstats);

            List<Chart> chartData = GetChartData(symbol);

            var ci = System.Globalization.CultureInfo.GetCultureInfo("en-us");
            List<DataPoint> dataPoints = new List<DataPoint>();
            foreach (Chart chart in chartData)
            {
                DateTime yourDateTime = DateTime.Parse(chart.date, ci);
                long yourDateTimeMilliseconds = new DateTimeOffset(yourDateTime).ToUnixTimeMilliseconds();
                DataPoint dp = new DataPoint(yourDateTimeMilliseconds, chart.high);
                dataPoints.Add(dp);
            }

            ViewBag.symbol = symbol;
            ViewBag.StockStat = new List<StockStats> { stockstats };
            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);

            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult StockGuide()
        {
            return View();
        }

        /*
            Save the available symbols in the database
        */
        public void PopulateSymbols(List<Company> companies)
        {
            // Retrieve the companies that were saved in the symbols method
            // List<Company> companies = JsonConvert.DeserializeObject<List<Company>>(TempData["Companies"].ToString());

            foreach (Company company in companies)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add company only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.Companies.Where(c => c.symbol.Equals(company.symbol)).Count() == 0)
                {
                    dbContext.Companies.Add(company);
                }
            }
            dbContext.SaveChanges();
        }

        /*
            Save the symbol statistic in the database
        */
        public void PopulateStockStats(StockStats stock)
        {
            //Database will give PK constraint violation error when trying to insert record with existing PK.
            //So add company only if it doesnt exist, check existence using symbol (PK)
            if (dbContext.StockStatistics.Where(c => c.symbol.Equals(stock.symbol)).Count() == 0)
            {
                dbContext.StockStatistics.Add(stock);
            }
            dbContext.SaveChanges();
        }

        /*
            Save the Symbol financial data in the database
        */
        public void PopulateSymbolFinancialData(SymbolFinancial symbolFinancial, String symbol)
        {
            // Database will give PK constraint violation error when trying to insert record with existing PK.
            // So add financial data only if it doesnt exist
            // check existence using symbol (Composite PK) and reportDate (Composite  PK)

            SymbolFinancial sym = new SymbolFinancial();
            sym.symbol = symbol;
            if (dbContext.SymbolFinancials.Where(c => c.symbol.Equals(symbol)).Count() == 0)
            {
                dbContext.SymbolFinancials.Add(sym);
            }
            dbContext.SaveChanges();

            if (symbolFinancial.financials == null) {
                return;
            }

            foreach (Financials fin in symbolFinancial.financials) {

                if (dbContext.Financials.Where(c => c.symbol.Equals(symbol)
                    && c.reportdate.Equals(fin.reportdate)).Count() == 0)
                {
                    fin.symbol = symbol;
                    dbContext.Financials.Add(fin);
                }
                dbContext.SaveChanges();
            }           
        }
    }
}
