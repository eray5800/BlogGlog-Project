using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.ElasticSearch
{
    public interface IElasticSearchService
    {
        Task<bool> CreateBlogAsync(Blog blog); // Create or Update
        Task<IEnumerable<Blog>> SearchBlogsAsync(string query); // Read

        Task<IEnumerable<Blog>> SearchBlogCategoryAsync(string category);
        Task<bool> UpdateBlogAsync(Blog blog); // Update
        Task<bool> DeleteBlogAsync(Guid blogId);
    }
}
