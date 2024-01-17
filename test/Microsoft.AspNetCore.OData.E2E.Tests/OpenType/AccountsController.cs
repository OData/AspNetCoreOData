//-----------------------------------------------------------------------------
// <copyright file="AccountsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.OpenType
{
    public class AccountsController : ODataController
    {
        static AccountsController()
        {
            InitAccounts();
        }

        /// <summary>
        /// static so that the data is shared among requests.
        /// </summary>
        public static IList<Account> Accounts = null;

        private static void InitAccounts()
        {
            Accounts = new List<Account>
            {
                new PremiumAccount()
                {
                    Id = 1,  // #1 is used for query
                    Name = "Name1",
                    AccountInfo = new AccountInfo()
                    {
                        NickName = "NickName1"
                    },
                    Address = new GlobalAddress()
                    {
                        City = "Redmond",
                        Street = "1 Microsoft Way",
                        CountryCode="US"
                    },
                    Tags = new Tags(),
                    Since=new DateTimeOffset(new DateTime(2014,5,22),TimeSpan.FromHours(8)),
                },
                new Account()
                {
                    Id = 2, // #2 is used for patch & Put
                    Name = "Name2",
                    AccountInfo = new AccountInfo()
                    {
                        NickName = "NickName2"
                    },
                    Address =  new Address()
                    {
                        City = "Shanghai",
                        Street = "Zixing Road"
                    },
                    Tags = new Tags()
                },
                new Account()
                {
                    Id = 3,
                    Name = "Name3",
                    AccountInfo = new AccountInfo()
                    {
                        NickName = "NickName3"
                        
                    },
                    Address = new Address()
                    {
                        City = "Beijing",
                        Street = "Danling Street"
                    },
                    Tags = new Tags()
                }
            };

            Account account = Accounts.Single(a => a.Id == 1);
            account.DynamicProperties["OwnerAlias"] = "jinfutan";
            account.DynamicProperties["OwnerGender"] = Gender.Female;
            account.DynamicProperties["IsValid"] = true;
            account.DynamicProperties["ShipAddresses"] = new List<Address>(){
                new Address
                {
                    City = "Beijing",
                    Street = "Danling Street"
                },
                new Address
                {
                    City="Shanghai",
                    Street="Zixing",
                }
            };

            Accounts[0].AccountInfo.DynamicProperties["Age"] = 10;

            Accounts[0].AccountInfo.DynamicProperties["Gender"] = Gender.Male;

            Accounts[0].AccountInfo.DynamicProperties["Subs"] = new string[] { "Xbox", "Windows", "Office" };

            Accounts[0].Address.DynamicProperties["CountryOrRegion"] = "US";
            Accounts[0].Tags.DynamicProperties["Tag1"] = "Value 1";
            Accounts[0].Tags.DynamicProperties["Tag2"] = "Value 2";

            Accounts[1].AccountInfo.DynamicProperties["Age"] = 20;

            Accounts[1].AccountInfo.DynamicProperties["Gender"] = Gender.Female;

            Accounts[1].AccountInfo.DynamicProperties["Subs"] = new string[] { "Xbox", "Windows" };

            Accounts[1].Address.DynamicProperties["CountryOrRegion"] = "AnyCountry";
            Accounts[1].Tags.DynamicProperties["Tag1"] = "abc";

            Accounts[2].AccountInfo.DynamicProperties["Age"] = 30;

            Accounts[2].AccountInfo.DynamicProperties["Gender"] = Gender.Female;

            Accounts[2].AccountInfo.DynamicProperties["Subs"] = new string[] { "Windows", "Office" };

            Accounts[2].Address.DynamicProperties["CountryOrRegion"] = "AnyCountry";
        }

        #region Get ~/.../Accounts
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IActionResult Get()
        {
            return Ok(Accounts.AsQueryable());
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        [HttpGet("/attributeRouting/Accounts")]
        public IActionResult GetAttributeRouting()
        {
            return Ok(Accounts.AsQueryable());
        }
        #endregion

        #region Get ~/.../Accounts({key})
        [HttpGet]
        public IActionResult Get(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key));
        }

        [HttpGet("/attributeRouting/Accounts({key})")]
        public IActionResult GetAttributeRouting(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key));
        }
        #endregion

        [EnableQuery]
        public IActionResult GetAccountsFromPremiumAccount()
        {
            return Ok(Accounts.OfType<PremiumAccount>().AsQueryable());
        }

        // This action has 9 endpoints
        // ~/attributeRouting/Accounts({key})/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.PremiumAccount/Since
        // ~/convention|explicit/Accounts({key})/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.PremiumAccount/Since
        // ~/convention|explicit/Accounts/{key}/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.PremiumAccount/Since
        // ~/convention|explicit/Accounts({key})/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.PremiumAccount/Since/$value
        // ~/convention|explicit/Accounts/{key}/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.PremiumAccount/Since/$value
        [HttpGet("/attributeRouting/Accounts({key})/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.PremiumAccount/Since")]
        public IActionResult GetSinceFromPremiumAccount(int key)
        {
            return Ok(Accounts.OfType<PremiumAccount>().SingleOrDefault(e => e.Id == key).Since);
        }

        // This action has 4 endpoints
        // ~/convention|explicit/Accounts({key})/AccountInfo
        // ~/convention|explicit/Accounts/{key}/AccountInfo
        public IActionResult GetAccountInfoFromAccount(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).AccountInfo);
        }

        #region ~/.../Accounts({key})/Address
        [HttpGet("/attributeRouting/Accounts({key})/Address")]
        public IActionResult GetAddressAttributeRouting(int key)
        {
            return GetAddress(key);
        }

        // convention routing
        public IActionResult GetAddress(int key)
        {
            Account account = Accounts.SingleOrDefault(e => e.Id == key);
            if (account == null)
            {
                return NotFound();
            }

            if (account.Address == null)
            {
                return this.StatusCode(StatusCodes.Status204NoContent);
            }

            return Ok(account.Address);
        }
        #endregion

        // This action only has the attribute routing.
        [HttpGet("/attributeRouting/Accounts({key})/Address/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GlobalAddress")]
        public IActionResult GetGlobalAddress(int key)
        {
            Address address = Accounts.SingleOrDefault(e => e.Id == key).Address;
            return Ok(address as GlobalAddress);
        }

        [HttpGet]
        public IActionResult GetAddressOfGlobalAddressFromAccount(int key)
        {
            Address address = Accounts.SingleOrDefault(e => e.Id == key).Address;
            return Ok(address as GlobalAddress);
        }

        [HttpGet("/attributeRouting/Accounts({key})/Address/City")]
        public IActionResult GetCityAttributeRouting(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).Address.City);
        }

        public IActionResult GetTagsFromAccount(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).Tags);
        }

        [HttpGet("/attributeRouting/Accounts({key})/Tags")]
        public IActionResult GetTagsAttributeRouting(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).Tags);
        }

        [HttpPatch]
        public IActionResult Patch(int key, [FromBody]Delta<Account> patch, ODataQueryOptions<Account> queryOptions)
        {
            return PatchAttributeRouting(key, patch, queryOptions);
        }

        [HttpPatch("/attributeRouting/Accounts({key})")]
        public IActionResult PatchAttributeRouting(int key, [FromBody]Delta<Account> patch, ODataQueryOptions<Account> queryOptions)
        {
            IEnumerable<Account> appliedAccounts = Accounts.Where(a => a.Id == key);

            if (appliedAccounts.Count() == 0)
            {
                return BadRequest(string.Format("The entry with Id {0} doesn't exist", key));
            }

            if (queryOptions.IfMatch != null)
            {
                IQueryable<Account> ifMatchAccounts = queryOptions.IfMatch.ApplyTo(appliedAccounts.AsQueryable()).Cast<Account>();

                if (ifMatchAccounts.Count() == 0)
                {
                    return BadRequest(string.Format("The entry with Id {0} has been updated", key));
                }
            }

            Account account = appliedAccounts.Single();
            patch.Patch(account);

            return Ok(account);
        }

        [HttpPut]
        public IActionResult Put(int key, [FromBody]Account account)
        {
            return PutAttributeRouting(key, account);
        }

        [HttpPut("/attributeRouting/Accounts({key})")]
        public IActionResult PutAttributeRouting(int key, [FromBody]Account account)
        {
            if (key != account.Id)
            {
                return BadRequest("The ID of customer is not matched with the key");
            }

            Account originalAccount = Accounts.Where(a => a.Id == account.Id).Single();
            Accounts.Remove(originalAccount);
            Accounts.Add(account);
            return Ok(account);
        }

        [HttpPost("/attributeRouting/Accounts")]
        public IActionResult Post([FromBody]Account account)
        {
            account.Id = Accounts.Count + 1;
            Accounts.Add(account);

            return Created(account);
        }

        [HttpDelete("/attributeRouting/Accounts({key})")]
        public IActionResult Delete(int key)
        {
            IEnumerable<Account> appliedAccounts = Accounts.Where(c => c.Id == key);

            if (appliedAccounts.Count() == 0)
            {
                return BadRequest(string.Format("The entry with ID {0} doesn't exist", key));
            }

            Account account = appliedAccounts.Single();
            Accounts.Remove(account);
            return this.StatusCode(StatusCodes.Status204NoContent);
        }

        // This action has two endpoints:
        // One is from Convention routing
        // the other is from attribute routing.
        [HttpPatch("/attributeRouting/Accounts({key})/Address")]
        public IActionResult PatchToAddress(int key, [FromBody]Delta<Address> address)
        {
            Account account = Accounts.FirstOrDefault(a => a.Id == key);
            if (account == null)
            {
                return NotFound();
            }

            if (account.Address == null)
            {
                account.Address = new Address();
            }

            account.Address = address.Patch(account.Address);

            return Updated(account);
        }

        // This action has two endpoints:
        // One is from Convention routing
        // the other is from attribute routing.
        [HttpPatch("/attributeRouting/Accounts({key})/Address/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GlobalAddress")]
        public IActionResult PatchToAddressOfGlobalAddress(int key, [FromBody]Delta<GlobalAddress> address)
        {
            Account account = Accounts.FirstOrDefault(a => a.Id == key);
            if (account == null)
            {
                return NotFound();
            }

            if (account.Address == null)
            {
                account.Address = new GlobalAddress();
            }

            GlobalAddress globalAddress = account.Address as GlobalAddress;
            account.Address = address.Patch(globalAddress);
            return Updated(account);
        }

        // This action has two endpoints:
        // One is from Convention routing
        // the other is from attribute routing.
        [HttpPut("/attributeRouting/Accounts({key})/Address")]
        public IActionResult PutToAddress(int key, [FromBody]Delta<Address> address)
        {
            Account account = Accounts.FirstOrDefault(a => a.Id == key);
            if (account == null)
            {
                return NotFound();
            }

            if (account.Address == null)
            {
                account.Address = new Address();
            }

            address.Put(account.Address);

            return Updated(account);
        }

        [HttpDelete("/attributeRouting/Accounts({key})/Address")]
        public IActionResult DeleteToAddress(int key)
        {
            Account account = Accounts.FirstOrDefault(a => a.Id == key);
            if (account == null)
            {
                return NotFound();
            }

            account.Address = null;
            return Updated(account);
        }

        #region Function & Action

        [HttpGet("/attributeRouting/Accounts({key})/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GetAddressFunction()")]
        public Address GetAddressFunctionOnAccount(int key)
        {
            return Accounts.SingleOrDefault(e => e.Id == key).Address;
        }

        [HttpPost("/attributeRouting/Accounts({key})/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.IncreaseAgeAction")]
        public AccountInfo IncreaseAgeActionOnAccount(int key)
        {
            AccountInfo accountInfo = Accounts.SingleOrDefault(e => e.Id == key).AccountInfo;
            accountInfo.DynamicProperties["Age"] = (int)accountInfo.DynamicProperties["Age"] + 1;
            return accountInfo;
        }

        [HttpPost("/attributeRouting/UpdateAddressAction")]
        public Address UpdateAddressActionAttributeRouting([FromBody]ODataActionParameters parameters)
        {
            var id = (int)parameters["ID"];
            var address = parameters["Address"] as Address;

            Account account = Accounts.Single(a => a.Id == id);
            account.Address = address;
            return address;
        }

        [HttpPost("Accounts({key})/Microsoft.Test.E2E.AspNet.OData.OpenType.AddShipAddress")]
        public IActionResult AddShipAddress(int key, [FromBody]ODataActionParameters parameters)
        {
            Account account = Accounts.Single(c => c.Id == key);
            if (account.DynamicProperties["ShipAddresses"] == null)
            {
                account.DynamicProperties["ShipAddresses"] = new List<Address>();
            }

            IList<Address> addresses = (IList<Address>)account.DynamicProperties["ShipAddresses"];
            addresses.Add(parameters["address"] as Address);
            return Ok(addresses.Count);
        }

        [HttpGet("Accounts({key})/Microsoft.Test.E2E.AspNet.OData.OpenType.GetShipAddresses")]
        public IActionResult GetShipAddresses(int key)
        {
            Account account = Accounts.Single(c => c.Id == key);
            if (account.DynamicProperties["ShipAddresses"] == null)
            {
                return Ok(new List<Address>());
            }
            else
            {
                IList<Address> addresses = (IList<Address>)account.DynamicProperties["ShipAddresses"];
                return Ok(addresses);
            }
        }
        #endregion

        [HttpPost("ResetDataSource")]
        public IActionResult ResetDataSource()
        {
            InitAccounts();
            return this.StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
