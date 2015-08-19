﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LetsEncrypt.ACME.JOSE;
using System.IO;
using System.Net;

namespace LetsEncrypt.ACME
{
    [TestClass]
    public class AcmeClientUnitTests
    {
        Uri _rootUrl = new Uri("http://acme2.aws3.ezshield.ws:4000/");

        [TestMethod]
        public void TestInit()
        {
            var client = new AcmeClient();
            client.RootUrl = _rootUrl;

            client.Init();
        }

        [TestMethod]
        public void TestGetDirectory()
        {
            var client = new AcmeClient();
            client.RootUrl = _rootUrl;

            var acmeDir = client.GetDirectory();
        }


        [TestMethod]
        public void TestRegister()
        {
            using (var signer = new RS256Signer())
            {
                signer.Init();
                using (var client = new AcmeClient())
                {
                    client.RootUrl = _rootUrl;
                    client.Signer = signer;
                    client.Init();

                    client.Register(new string[]
                    {
                        "mailto:letsencrypt@mailinator.com",
                        "tel:+14109361212",
                    });

                    Assert.IsNotNull(client.Registration);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(client.Registration.RegistrationUri));

                    using (var fs = new FileStream("..\\TestRegister.acmeReg", FileMode.Create))
                    {
                        client.Registration.Save(fs);
                    }
                }
                using (var fs = new FileStream("..\\TestRegister.acmeSigner", FileMode.Create))
                {
                    signer.Save(fs);
                }
            }
        }

        [TestMethod]
        public void TestRegisterDuplicate()
        {
            using (var signer = new RS256Signer())
            {
                signer.Init();
                using (var fs = new FileStream("..\\TestRegisterDuplicate.acmeSigner", FileMode.Open))
                {
                    signer.Load(fs);
                }

                using (var client = new AcmeClient())
                {
                    client.RootUrl = _rootUrl;
                    client.Signer = signer;
                    client.Init();

                    try
                    {
                        client.Register(new string[]
                        {
                        "mailto:letsencrypt+dup@mailinator.com",
                        "tel:+14105551212",
                        });
                        Assert.Fail("WebException expected");
                    }
                    catch (WebException ex)
                    {
                        var resp = ex.Response as HttpWebResponse;
                        Assert.IsNotNull(resp);
                        Assert.AreEqual(HttpStatusCode.Conflict, resp.StatusCode);
                    }
                }
            }
        }

        public void TestAuthorizeDns()
        {
            using (var signer = new RS256Signer())
            {
                signer.Init();
                using (var fs = new FileStream("..\\TestRegister.acmeSigner", FileMode.Open))
                {
                    signer.Load(fs);
                }

                AcmeRegistration reg;
                using (var fs = new FileStream("..\\TestRegister.acmeReg", FileMode.Open))
                {
                    reg = AcmeRegistration.Load(fs);
                }

                using (var client = new AcmeClient())
                {
                    client.RootUrl = _rootUrl;
                    client.Signer = signer;
                    client.Registration = reg;
                    client.Init();
                }
            }
        }
    }
}
