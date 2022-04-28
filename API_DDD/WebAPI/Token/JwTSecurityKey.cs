﻿using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebAPI.Token
{
    public class JwTSecurityKey
    {
        public static SymmetricSecurityKey Create(string secret)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }

    }
}
