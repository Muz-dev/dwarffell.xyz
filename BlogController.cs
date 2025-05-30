using Microsoft.AspNetCore.Mvc;
using ServerList;
using System;
using System.IO;
using System.Security.Cryptography;
namespace LunaSite.Blog
{
    [Route("api/admin/blogcontroller")]
    [ApiController]
    public class BlogController: ControllerBase
    {
        [HttpGet("posts")]
        public IActionResult GetAllPosts()
        {
            List<Post> posts = BlogService.GetAllPosts();
            string s = string.Empty;
            foreach(Post post in posts.AsEnumerable().Reverse())
			{
                s += $"<div class='blogpost'>" +
                    $"<span class='blogtitle'><a href='/Blog/{post.id}'>{post.Title}</a></span><br/>" +
                    $"<span class='blogmessage'>{post.Content}</span><br/>";
                if (post.ImageSources != null && post.ImageSources.Count >= 1)
                {
                    foreach(string image in post.ImageSources)
					{
						s += $"<img src='{image}' class='blogimagesmall'/>";
					}
				}
                s += $"<br/><span class='blogfooter'>{post.Author} - {post.TimeStamp.ToString("dd MMM yyyy HH:mm:ss")}</span></div>";
			}
            return Ok(new { message = s });
		}

        [HttpPost("savepost")]
        public IActionResult SavePost([FromForm] WriteRoot root, [FromForm] List<IFormFile> postImageSources)
        {
            Console.WriteLine($"{Request.HttpContext.Connection.RemoteIpAddress?.ToString()}-{root.ToString()}");
			if (root == null)
            {
				return BadRequest(new { message = "Invalid Request" });
			}

            if (root.user == AuthTokens.AdminUser && root.pass == AuthTokens.AdminPass)
            {
				if (root.post == null)
				{
					return BadRequest(new { message = "Post is required" });
				}
                root.post.id = BlogService.GetNextPostId();
                root.post.TimeStamp = DateTime.Now;

                List<string> images = new List<string>();
                foreach (var file in postImageSources)
                {
                    if (file.Length > 0)
                    {
                        string filePath = $"wwwroot/blogimages/{file.FileName}";
                        using (var stream = new FileStream(filePath, FileMode.Create))
						{
							file.CopyTo(stream);
						}
                        images.Add($"blogimages/{file.FileName}");
                        Console.WriteLine($"Saved image {file.FileName} to {filePath}");
					}
                }
                root.post.ImageSources = images;
				BlogService.SavePost(root.post);
				return Ok(new { message = "Post saved" });
			}
			else
			{
				return BadRequest(new { message = "Invalid credentials" });
			}
        }
        public static ulong GetRandomULong()
        {
            byte[] buffer = new byte[4];
            RandomNumberGenerator.Fill(buffer);
            return BitConverter.ToUInt64(buffer, 0);
		}
	}
}