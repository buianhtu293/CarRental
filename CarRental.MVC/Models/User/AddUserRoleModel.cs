using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using CarRental.Domain.Entities;

namespace App.Areas.Identity.Models.UserViewModels
{
  public class AddUserRoleModel
  {
    public User user { get; set; }

    [DisplayName("Các role gán cho user")]
    public string[] RoleNames { get; set; }

    public List<IdentityRoleClaim<Guid>> claimsInRole { get; set; }
    public List<IdentityUserClaim<Guid>> claimsInUserClaim { get; set; }

  }
}