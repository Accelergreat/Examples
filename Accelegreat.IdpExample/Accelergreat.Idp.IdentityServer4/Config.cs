// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace Accelergreat.Idp.IdentityServer4
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("accelergreat")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
               // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "accelergreat",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:4200/auth/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:4200/auth/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:4200/auth/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "accelergreat" }
                }
            };
    }
}