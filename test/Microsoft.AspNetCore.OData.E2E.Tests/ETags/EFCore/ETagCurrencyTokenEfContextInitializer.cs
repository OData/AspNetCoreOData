//-----------------------------------------------------------------------------
// <copyright file="ETagCurrencyTokenEfContextInitializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ETags.EFCore;

public class ETagCurrencyTokenEfContextInitializer
{
    private static IEnumerable<Dominio> _dominios;
    private static IEnumerable<Server> _servers;

    private static IEnumerable<Dominio> Dominios
    {
        get
        {
            if (_dominios == null)
            {
                Generate();
            }

            Assert.NotNull(_dominios);
            return _dominios;
        }
    }

    private static IEnumerable<Server> Servers
    {
        get
        {
            if (_servers == null)
            {
                Generate();
            }

            Assert.NotNull(_servers);
            return _servers;
        }
    }

    private static void Generate()
    {
        if (_dominios != null || _servers != null)
        {
            return;
        }

        List<Server> servers = new List<Server>(2);
        Server server1 = new Server
        {
            Id = "1",
            Descrizione = "Server 1",
            Url = "http://server1",
            RECVER = null
        };
        servers.Add(server1);

        Server server2 = new Server
        {
            Id = "2",
            Descrizione = "Server 2",
            Url = "http://server2",
            RECVER = 5
        };
        servers.Add(server2);
        _servers = servers;

        List<Dominio> dominios = new List<Dominio>(2);
        Dominio do1 = new Dominio
        {
            Id = "1",
            ServerAutenticazione = server1,
            Descrizione = "Test1",
            RECVER = null,
            ServerAutenticazioneId = "1",
        };
        dominios.Add(do1);

        Dominio do2 = new Dominio
        {
            Id = "2",
            ServerAutenticazione = server2,
            Descrizione = "Test2",
            RECVER = 10,
            ServerAutenticazioneId = "2",
        };

        dominios.Add(do2);
        _dominios = dominios;
    }

    public static void Seed(ETagCurrencyTokenEfContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Dominios.Any())
        {
            Generate();

            foreach (var d in Dominios)
            {
                context.Dominios.Add(d);
            }

            foreach (var s in Servers)
            {
                context.Servers.Add(s);
            }

            context.SaveChanges();
        }
    }
}
