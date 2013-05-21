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
                // Have both unique id AND email (Google OpenID)
                if (!string.IsNullOrEmpty(login.UniqueId) && !string.IsNullOrEmpty(login.Email))
                {
                    // In this case, try to lookup the email first for an existing account
                    UserData userData = entities.UserDatas.SingleOrDefault(x => x.Email == login.Email);

                    if (userData == null) // Create user with key and email
                    {
                        var response = AddUser(login.Email, login.UniqueId, new PasswordHash(login.UniqueId), true);
                        return (!response.Error) ? GetUser(login) : null;
                    }
                    return new User(userData);
                }

                // Have just a unique id (Facebook Connect or OpenId w/ no email given)
                if (!string.IsNullOrEmpty(login.UniqueId))
                {
                    // In this case, try to lookup the unique id
                    UserData userData = entities.UserDatas.SingleOrDefault(x => x.OpenId == login.UniqueId);

                    if (userData == null) // Create user based only on key
                    {
                        var response = AddUser(null, login.UniqueId, new PasswordHash(login.UniqueId));
                        return (!response.Error) ? GetUser(login) : null;
                    }
                    return new User(userData);
                }

                // Have just an email (Teamkeep login)
                if (!string.IsNullOrEmpty(login.Email))
                {
                    // In this case, look up by the email only
                    UserData userData = entities.UserDatas.SingleOrDefault(x => x.Email == login.Email);

                    // User exists? If it does not exist, login will fail
                    if (userData == null || userData.Password == null) return null;

                    // Password matches?
                    var password = new PasswordHash(userData.Password);
                    if (password.Verify(login.Password)) return new User(userData);
                }

                return null; // No cases matched; login fails
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

        public ServiceResponse AddUser(string email, string uniqueId, PasswordHash passwordHash, bool verified = false)
        {
            using (var entities = Database.GetEntities())
            {
                if(!string.IsNullOrEmpty(email))
                {
                    var existingEmail = entities.UserDatas.SingleOrDefault(x => x.Email == email);
                    if (existingEmail != null) // Email in use
                    {
                        return new ServiceResponse { Error = true, Message = "Email is already in use" };
                    }
                }

                var userData = new UserData
                {
                    OpenId = uniqueId,
                    Username = ConvertEmailToUsername(email),
                    Email = email,
                    Password = passwordHash.ToArray()
                };
                if (!verified)
                {
                    userData.Verify = AuthToken.GenerateKey(email); // Now unverified
                }
                entities.UserDatas.AddObject(userData);
                entities.SaveChanges();

                return new ServiceResponse();
            }
        }

        private string ConvertEmailToUsername(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;
            return new Regex("[^A-Za-z0-9]").Replace(email, "");
        }
    }
}