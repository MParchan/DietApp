using DietApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DietApp.Data
{
    public class DbInitializer
    {
        
        public static void Initialize(ApplicationDbContext context)
        {
            string AdminRoleId = Guid.NewGuid().ToString();
            string AdminRoleConcurrencyStamp = Guid.NewGuid().ToString();

            context.Database.EnsureCreated();
            
            if (context.Roles.Any())
            {
                return;
            }
            

            var roles = new ApplicationRole[]
            {
                new ApplicationRole{Id=AdminRoleId,Name="Admin",NormalizedName="ADMIN",ConcurrencyStamp=AdminRoleConcurrencyStamp}
            };
            foreach (ApplicationRole r in roles)
            {
                context.Roles.Add(r);
            }
            context.SaveChanges();

            string User1Id = Guid.NewGuid().ToString();
            string User2Id = Guid.NewGuid().ToString();
            string User1SecurityStamp = Guid.NewGuid().ToString();
            string User2SecurityStamp = Guid.NewGuid().ToString();
            string User1ConcurrencyStamp = Guid.NewGuid().ToString();
            string User2ConcurrencyStamp = Guid.NewGuid().ToString();

            var users = new ApplicationUser[]
            {
                new ApplicationUser{Id=User1Id,Height=0,Weight=0,Age=0,Gender=0,Activity=0,UserName="admin@admin.pl",NormalizedUserName="ADMIN@ADMIN.PL",Email="admin@admin.pl",NormalizedEmail="ADMIN@ADMIN.PL",EmailConfirmed=false,PasswordHash="AQAAAAEAACcQAAAAEF48qagIvzo0Yru1DduXLO1b52ty4xVIZeaTbPOFRBJ2rAgaqwUmTsJRJ3nR5gt6Fg==",SecurityStamp=User1SecurityStamp,ConcurrencyStamp=User1ConcurrencyStamp,PhoneNumber=null,PhoneNumberConfirmed=false,TwoFactorEnabled=false,LockoutEnd=null,LockoutEnabled=true,AccessFailedCount=0},
                new ApplicationUser{Id=User2Id,Height=0,Weight=0,Age=0,Gender=0,Activity=0,UserName="user@user.pl",NormalizedUserName="USER@USER.PL",Email="user@user.pl",NormalizedEmail="USER@USER.PL",EmailConfirmed=false,PasswordHash="AQAAAAEAACcQAAAAEH+BZHzAEblKHjDDz0MMmZO0iEMh+87+uLUoPMg0NR48L/JOEkBB5Tw7yK5xNiqsjw==",SecurityStamp=User2SecurityStamp,ConcurrencyStamp=User2ConcurrencyStamp,PhoneNumber=null,PhoneNumberConfirmed=false,TwoFactorEnabled=false,LockoutEnd=null,LockoutEnabled=true,AccessFailedCount=0}       
            };
            foreach (ApplicationUser u in users)
            {
                context.Users.Add(u);
                
            }
            context.SaveChanges();

        }
    }
}
