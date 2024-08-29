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
        Task IndexBlogAsync(Blog blog); // Create or Update
        Task<IEnumerable<Blog>> SearchBlogsAsync(string query); // Read
        Task<bool> UpdateBlogAsync(Blog blog); // Update
        Task<bool> DeleteBlogAsync(string blogId); // Delete
    }
}
