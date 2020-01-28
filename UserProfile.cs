// stores user's profile 
using System.Collections.Generic;

public class UserProfile
{
    public string Name { get; set; }
  
    // The list of companies the user wants to review.
    public List<string> ModulesTaken { get; set; } = new List<string>();
}