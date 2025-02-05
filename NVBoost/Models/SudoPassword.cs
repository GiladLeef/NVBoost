using System;
using System.Security;

namespace nvboost.Models;

public class SudoPassword
{

    public string Password { get; private set; }
    public DateTime ExpirationTime { get; private set; }
    public bool IsExpired => ExpirationTime < DateTime.Now;

    public SudoPassword(string password, DateTime expirationTime)
    {
        Password = password;
        ExpirationTime = expirationTime;
    }

    public SudoPassword(string password, TimeSpan timeToExpiration)
    {
        Password = password;
        ExpirationTime = DateTime.Now.Add(timeToExpiration);
    }

    public SudoPassword(string password)
    {
        Password = password;
        ExpirationTime = DateTime.MaxValue;
    }
}