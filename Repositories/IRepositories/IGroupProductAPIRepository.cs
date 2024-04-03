using Entities.ViewModels.ArticlesAPI;

namespace Repositories.IRepositories
{
    public interface IGroupProductAPIRepository
    {
       public Task<string> GetGroupProductName(int cateID);
        public Task<List<ArticleGroupViewModel>> GetArticleCategoryByParentID(long parent_id);
        public Task<List<ArticleGroupViewModel>> GetFooterCategoryByParentID(long parent_id);
        public Task<List<ProductGroupViewModel>> GetProductGroupByParentID(long parent_id, string url_static);

    }
}
