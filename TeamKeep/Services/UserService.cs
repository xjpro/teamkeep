using System.Linq;
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
                // OpenID or Facebook login
                if (!string.IsNullOrEmpty(login.UniqueId)) 
                {
                    return GetOpenIdUser(login);
                }

                // Password login
                if (!string.IsNullOrEmpty(login.Username)) 
                {
                    var userData = entities.UserDatas.SingleOrDefault(x => x.Username == login.Username);
                    
                    if (userData == null || userData.Password == null) return null;

                    // Password matches?
                    var password = new PasswordHash(userData.Password);
                    if (password.Verify(login.Password)) return new User(userData);
                }

                return null; // No cases matched; login fails
            }
        }

        private User GetOpenIdUser(Login login)
        {
            using (var entities = Database.GetEntities())
            {
                var userData = entities.UserDatas.SingleOrDefault(x => x.LoginId == login.UniqueId);
                if (userData != null)
                {
                    return new User(userData);
                }

                var response = AddUser(new UserData { Username = null, Email = login.Email, LoginId = login.UniqueId }, new PasswordHash(login.UniqueId), login.Email != null);
                if (!response.Error) return GetUser(login);
                return null;
            }
        }

        public User GetUser(PasswordReset passwordReset)
        {
            using (var entities = Database.GetEntities())
            {
                var userData = entities.UserDatas.SingleOrDefault(x => x.Username == passwordReset.Username);
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

        public bool VerifyEmail(string verifyCode)
        {
            using (var entities = Database.GetEntities())
            {
                var userData = entities.UserDatas.SingleOrDefault(x => x.Verify == verifyCode);
                if (userData != null)
                {
                    userData.Verify = null;
                    entities.SaveChanges();
                    return true;
                }
                return false;
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
                userData.Verify = null;
                entities.SaveChanges();
                return new User(userData);
            }
        }

        public User UpdateUserEmail(int userId, string email)
        {
            using (var entities = Database.GetEntities())
            {
                var userData = entities.UserDatas.Single(x => x.Id == userId);
                userData.Email = email;
                userData.Verify = AuthToken.GenerateKey(email); // Now unverified
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

        public ServiceResponse AddUser(UserData user, PasswordHash passwordHash, bool verified = false)
        {
            using (var entities = Database.GetEntities())
            {
                if(!string.IsNullOrEmpty(user.Username))
                {
                    var existingUsername = entities.UserDatas.SingleOrDefault(x => x.Username == user.Username);
                    if (existingUsername != null) // Username in use
                    {
                        return new ServiceResponse { Error = true, Message = "Username is already in use" };
                    }
                }
                // TODO check for open id in use

                var userData = new UserData
                {
                    LoginId = user.LoginId,
                    Username = user.Username,
                    Email = user.Email,
                    Password = passwordHash.ToArray()
                };
                if (!verified)
                {
                    userData.Verify = AuthToken.GenerateKey(user.Email); // Now unverified
                }
                entities.UserDatas.AddObject(userData);
                entities.SaveChanges();

                return new ServiceResponse();
            }
        }
    }
}