using AutoMapper;
using NLog;
using PMS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.DAL.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly Database.PMSEntities _dbContext;
        private readonly Logger loggerFile;
        private readonly Logger loggerDB;
        //Controller
        public UserRepository()
        {
            _dbContext = new Database.PMSEntities();
            loggerFile = LogManager.GetCurrentClassLogger();//log into file
            loggerDB = LogManager.GetLogger("databaseLogger");//log into database
        }

        public List<string> AuthenticateUser(UserData model)
        {
            var entity = _dbContext.UserDatas.Where(x => x.Email.Equals(model.Email) && x.Password.Equals(model.Password)).FirstOrDefault();//Finds User
            List<string> session = new List<string>();
            if (entity != null)
            {
                session.Add(entity.Name);
                session.Add(entity.Id.ToString());
                return session;
            }
            session.Add("Not Found");
            return session;
        }
       
        public List<string> CreateUser(UserData model)
        {
            List<string> session = null;
            try
            {
                if (model != null)
                {
                    session = new List<string>();
                    Database.UserData entity = new Database.UserData();
                    var config = new MapperConfiguration(cfg => cfg.CreateMap<UserData, Database.UserData>());
                    var Mapper = new Mapper(config);
                    entity = Mapper.Map<Database.UserData>(model);
                    entity.CreatedDate = DateTime.Now;
                    _dbContext.UserDatas.Add(entity);
                    _dbContext.SaveChanges();
                    session.Add(entity.Name);
                    session.Add(entity.Id.ToString());
                    return session;
                }
                return session;
            }
            catch (Exception ex)
            {
             
                return session;
            }
        }
    }
}
