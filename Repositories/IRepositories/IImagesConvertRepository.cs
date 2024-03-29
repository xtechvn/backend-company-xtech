using ENTITIES.ViewModels.Static;

namespace Repositories
{
    public interface IImagesConvertRepository
    {
        public Task<string> InsertImage(ImagesConvertMongoDbModel item);
        public ImagesConvertMongoDbModel GetImageByURL(string url);

    }
}
