﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Whois.NET;

namespace BenchmarkLab.Controllers
{
    public class ToolsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult JSONBeautify()
        {
            return View();
        }

        public IActionResult JSONMinify()
        {
            return View();
        }

        public IActionResult JavaScriptBeautify()
        {
            return View();
        }

        public IActionResult HTMLBeautify()
        {
            return View();
        }

        public IActionResult CSSBeautify()
        {
            return View();
        }

        public IActionResult FormatSQL()
        {
            return View();
        }

        public IActionResult SortLines()
        {
            return View();
        }

        public IActionResult ComputeFileHash()
        {
            return View();
        }

        public IActionResult ConvertUnixTimestamp()
        {
            return View();
        }

        public async Task<IActionResult> WhoisLookup(string domain)
        {
            ViewData["domain"] = domain;
            if (string.IsNullOrEmpty(domain))
            {
                return View();
            }

            try
            {
                var options = new WhoisQueryOptions
                {
                };
                var result = await WhoisClient.QueryAsync(domain, options);
                return View(result);
            }
            catch (Exception e)
            {
                ViewData["error"] = e.Message;
                return View();
            }
        }

        public IActionResult URLEncode()
        {
            return View();
        }

        public IActionResult URLDecode()
        {
            return View();
        }

        public IActionResult Base64Encode()
        {
            return View();
        }

        public IActionResult Base64Decode()
        {
            return View();
        }

        public async Task<IActionResult> GetIPAddressesByHostName(string host)
        {
            ViewData["host"] = host;
            if (String.IsNullOrWhiteSpace(host))
            {
                return View();
            }

            try
            {
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(host);
                return View(addresses);
            }
            catch (Exception e)
            {
                ViewData["error"] = e.Message;
                return View();
            }
        }

        public async Task<IActionResult> GetHostsByIPAddress(string ip)
        {
            ViewData["ip"] = ip;
            if (String.IsNullOrWhiteSpace(ip))
            {
                return View();
            }

            try
            {
                IPHostEntry entries = await Dns.GetHostEntryAsync(ip);
                return View(entries);
            }
            catch (Exception e)
            {
                ViewData["error"] = e.Message;
                return View();
            }
        }

        public IActionResult FormatHTML()
        {
            return View();
        }

        // User Agent String
        // Remote IP
        // Browser features
        // https://stackoverflow.com/questions/13798286/ip-address-of-the-user-who-is-browsing-my-website
        // https://github.com/rtfpessoa/diff2html#diff2htmlui-browser
        // Sort lines
    }
}