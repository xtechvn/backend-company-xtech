using ENTITIES.APPModels.Static;
using ENTITIES.ViewModels.Static;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Utilities;

namespace Repositories
{
    public class ImagesConvertRepository: IImagesConvertRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<ImagesConvertMongoDbModel> images_mongoCollection;

        public ImagesConvertRepository(IConfiguration configuration)
        {
            try
            {
                _configuration = configuration;
                var config = new MongoDbConfig()
                {
                    host = _configuration["DataBaseConfig:MongoServer:Host"],
                    port = Convert.ToInt32(_configuration["DataBaseConfig:MongoServer:Port"]),
                    user_name = _configuration["DataBaseConfig:MongoServer:user"],
                    password = _configuration["DataBaseConfig:MongoServer:pwd"],
                    database_name = _configuration["DataBaseConfig:MongoServer:catalog_core"]
                };
                //-- "mongodb://user1:password1@localhost/test"
                string url = "mongodb://" + config.user_name + ":" + config.password + "@" + config.host + ":" + config.port + "/?authSource=" + config.database_name;
                var client = new MongoClient(url);
                IMongoDatabase db = client.GetDatabase(config.database_name);
                var images_collection = _configuration["MongoServer:images_collection"];
                images_mongoCollection = db.GetCollection<ImagesConvertMongoDbModel>(images_collection);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BookingHotelDAL - BookingHotelDAL: " + ex);
                throw;
            }
        }
        public async Task<string> InsertImage(ImagesConvertMongoDbModel item)
        {
            try
            {
                item.GenID();
                await images_mongoCollection.InsertOneAsync(item);
                return item._id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertImage - ImagesConvertDAL - Cannot Excute: " + ex.ToString());
                return null;
            }
        }
        public ImagesConvertMongoDbModel GetImageByURL(string url)
        {
            try
            {
            
                var filter = Builders<ImagesConvertMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ImagesConvertMongoDbModel>.Filter.Eq(x => x.orginal_url, url);
               
                var model = images_mongoCollection.Find(filterDefinition).FirstOrDefault();
                if (model != null && model._id != null && model._id.Trim() != "")
                    return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetImageByURL - ImagesConvertDAL - Cannot Excute: " + ex.ToString());
            }
            return null;
        }
    }
}
