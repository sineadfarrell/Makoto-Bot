// stores user's profile 
using System;
using System.Collections.Generic;

public class UserProfile
{
    public string[] Name { get; set; }

    // The list of companies the user wants to review.

    public string[] NumberOfModules { get; set; }
    public string[] Module { get; set; }
    public string[] Stage { get; set; }
}