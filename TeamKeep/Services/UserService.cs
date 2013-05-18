using System.Linq;
using System.Text.RegularExpressions;
using TeamKeep.Models;
using TeamKeep.Models.DataModels;
using TeamKeep.Models.ServiceResponses;

namespace TeamKeep.Services
{
    public class UserService
    {
        public User GetUser(Login login)
        {
            using (var entities = Database.GetEntities())
            {
                var userData = entities.UserDatas.SingleOrDefault(x => x.Email == login.Email);

                // User exists?
                if (userData == null || userData.Password == null) return null;
                
                // Password matches?
                var password = new PasswordHash(userData.Password);
                if (!password.Verify(login.Password)) return null;

                return new User(userData);
            }
        }

        public User GetUser(string openId, string email)
        {
            using (var entities = Database.GetEntities())
            {
                if (!string.IsNullOrEmpty(email))
                {
                    var userData = entities.UserDatas.SingleOrDefault(x => x.Email == email);
                    if (userData == null) // Create user
                    {
                        userData = new UserData
                        {
                            Email = email,
                            Username = ConvertEmailToUsername(email)
                        };
                        entities.UserDatas.AddObject(userData);
                        entities.SaveChanges();
                    }

                    if (userData.OpenId == null) // Save open id
                    {
                        userData.OpenId = openId;
                        userData.Password = new PasswordHash(openId).ToArray();
                        entities.SaveChanges();
                    }

                    return new User(userData);
                }
                else
                {
                    var userData = entities.UserDatas.SingleOrDefault(x => x.OpenId == openId);
                    if (userData == null) // Create user
                    {
                        userData = new UserData
                        {
                            OpenId = openId,
                            Password = new PasswordHash(openId).ToArray()
                        };
                        entities.UserDatas.AddObject(userData);
                        entities.SaveChanges();
                    }
                    return new User(userData);
                }
            }
        }

        public User GetUser(PasswordReset passwordReset)
        {
            using (var entities = Database.GetEntities())
            {
                var userData = entities.UserDatas.SingleOrDefault(x => x.Email == passwordReset.Email);
                return (userData != null) ? new User(userData) : null;
            }
        }

        public User GetUser(AuthToken token)
        {
            using (var entities = Database.GetEntities())
            {
                var userData = entities.UserDatas.SingleOrDefault(x => x.Id == token.UserId);

                if(userData == null || !token.Verify(userData.Password)) return null; // Failed auth

                return new User(userData);
            }
        }

        public AuthToken GetAuthToken(int userId)
        {
            using (var entities = Database.GetEntities())
            {
                var userData = entities.UserDatas.Single(x => x.Id == userId);
                return AuthToken.Generate(userData.Id, userData.Password);
            }
        }

        public User UpdateUser(User user)
        {
            using (var entities = Database.GetEntities())
            {
                var userData = entities.UserDatas.Single(x => x.Id == user.Id);
                userData.LastSeen = user.LastSeen;
                entities.SaveChanges();
                return user;
            }
        }

        public User UpdateUserPassword(int userId, PasswordHash password)
        {
            using (var entities = Database.GetEntities())
            {
                var userData = entities.UserDatas.Single(x => x.Id == userId);
                userData.Password = password.ToArray();
                userData.Reset = null;
                entities.SaveChanges();
                return new User(userData);
            }
        }

        public void SetActiveTeamId(int userId, int teamId)
        {
            using (var entities = Database.GetEntities())
            {
                entities.UserDatas.Single(x => x.Id == userId).ActiveTeamId = teamId;
                entities.SaveChanges();
            }
        }

        public void SetResetHash(int userId, string resetHash)
        {
            using (var entities = Database.GetEntities())
            {
                entities.UserDatas.Single(x => x.Id == userId).Reset = resetHash;
                entities.SaveChanges();
            }
        }

        public ServiceResponse AddUser(User user, PasswordHash passwordHash)
        {
            using (var entities = Database.GetEntities())
            {
                var existingEmail = entities.UserDatas.SingleOrDefault(x => x.Email == user.Email);
                if (existingEmail != null) // Email in use
                {
                    return new ServiceResponse {Error = true, Message = "Email is already in use"};
                }

                entities.UserDatas.AddObject(new UserData
                {
                    Username = ConvertEmailToUsername(user.Email),
                    Email = user.Email,
                    Password = passwordHash.ToArray()
                });

                entities.SaveChanges();

                return new ServiceResponse();
            }
        }

        private string ConvertEmailToUsername(string email)
        {
            return new Regex("[^A-Za-z0-9]").Replace(email, "");
        }
    }
}