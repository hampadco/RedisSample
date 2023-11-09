using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace webapiredis.Controllers;

[ApiController]
[Route("[action]")]
public class HomeController : ControllerBase
{

    //redis
    private readonly IDistributedCache _distributedCache;

    //Context 
    private readonly Context _context;
    

    public HomeController(IDistributedCache distributedCache,Context context)
    {
        _distributedCache = distributedCache;
        _context = context;
    }

    //get value string 
    [HttpGet]
    public string Getinfo()
    {
        var value = _distributedCache.GetString("key");
        if(value !=null)
        {
            //read from redis
            return value;
        }else
        
        {
            //write to redis
            //expierd redis
            var options = new DistributedCacheEntryOptions();
            options.SetAbsoluteExpiration(DateTimeOffset.Now.AddSeconds(20));
            _distributedCache.SetString("key","found",options);
            return "not found";
        }
       


       
    }

    //add student 
    [HttpPost]
    public async Task<IActionResult> AddStudent(Student student)
    {
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
        return Ok(student);
    }

    //get all student

    [HttpGet]
    public async Task<IActionResult> GetStudent()
    {
       //redis

       var studentkey= await _distributedCache.GetStringAsync("keyst");
         if(studentkey !=null)
        {
            //read from redis

            var st = JsonConvert.DeserializeObject<List<Student>>(studentkey);
             System.Console.WriteLine("redis");
            return Ok(st);
            
        }
        else
        {
            //write to redis
            var st = await _context.Students.ToListAsync();
            var options = new DistributedCacheEntryOptions();
            options.SetAbsoluteExpiration(DateTimeOffset.Now.AddSeconds(20));
            await _distributedCache.SetStringAsync("keyst",JsonConvert.SerializeObject(st),options);
            System.Console.WriteLine("db");
            return Ok(st);
        }

        
    }



    
   
}
