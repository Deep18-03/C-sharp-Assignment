using AutoMapper;
using NLog;
using PMS.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PMS.DAL.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly Database.PMSEntities _dbContext;
        private readonly Logger loggerFile;
        private readonly Logger loggerDB;
        public ProductRepository()
        {
            _dbContext = new Database.PMSEntities();
            loggerFile = LogManager.GetCurrentClassLogger();//Log into File
            loggerDB = LogManager.GetLogger("databaseLogger");//Log into database
        }
        //Add Product.
        public string CreateProduct(ProductData model)
        {
            try
            {
                if (model != null)
                {
                    var entity = _dbContext.UserDatas.Find(model.UserId);//Find User
                    var config = new MapperConfiguration(cfg => cfg.CreateMap<ProductData, Database.ProductData>());
                    var Mapper = new Mapper(config);
                    Database.ProductData product = Mapper.Map<Database.ProductData>(model);
                    product.CreatedDate = DateTime.Now;//Created Date
                    entity.ProductDatas.Add(product);
                    _dbContext.SaveChanges();                   
                    return "Successfully Added!";
                }
                return "Model Is Null!";
            }
            catch (Exception ex)
            {
               
                return ex.Message;//return exception message
            }
        }
        //Delete Product. 
        public string DeleteProduct(int id)
        {
            try
            {
                var entity = _dbContext.ProductDatas.Find(id);//Find Product
                if (entity != null)//If product finds.
                {
                   
                    _dbContext.ProductDatas.Remove(entity);
                    _dbContext.SaveChanges();//Save database
                    return "Deleted!";
                }
                return "No data found";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //List Out Products 
        public List<ProductData> GetAllProducts(int id)
        {
            var entity = _dbContext.UserDatas.Find(id);

            List<ProductData> productList = new List<ProductData>();

            if (entity != null)
            {
                var config = new MapperConfiguration(cfg => cfg.CreateMap<Database.ProductData, ProductData>());
                var Mapper = new Mapper(config);
                productList = (List<ProductData>)Mapper.Map<List<ProductData>>(entity.ProductDatas); 
            }
            return productList;
        }
    
        public ProductData GetProduct(int id)
        {
            var entity = _dbContext.ProductDatas.Find(id);
            ProductData product = new ProductData();
            if (entity != null)
            {
                var config = new MapperConfiguration(cfg => cfg.CreateMap<Database.ProductData, ProductData>());
                var Mapper = new Mapper(config);
                product = Mapper.Map<ProductData>(entity);
            }
            return product;
        }

    }
}
