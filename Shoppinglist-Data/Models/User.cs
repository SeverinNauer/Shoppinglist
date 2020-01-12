using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Digitalist_Data.Helpers;

namespace Digitalist_Data.Models
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

        public User(string username, string password)
        {
            Username = username;
            Password = BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
