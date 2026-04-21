using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public DatabaseSeeder(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        // Check if data already exists
        if (await _context.Set<User>().AnyAsync())
        {
            Console.WriteLine("Database already contains data. Skipping seed.");
            return;
        }

        Console.WriteLine("Seeding database...");

        // Create users
        var users = new List<User>
        {
            CreateUser("johndoe", "john@example.com", "John Doe", "Software engineer passionate about tech and innovation. Love coding! 🚀", "Password123!"),
            CreateUser("janedoe", "jane@example.com", "Jane Doe", "Tech enthusiast | Coffee lover ☕ | Building cool stuff", "Password123!"),
            CreateUser("alice", "alice@example.com", "Alice Smith", "Designer & Developer | Making the web beautiful", "Password123!"),
            CreateUser("bob", "bob@example.com", "Bob Johnson", "Full-stack developer | Open source contributor 💻", "Password123!"),
            CreateUser("charlie", "charlie@example.com", "Charlie Brown", "Data scientist exploring AI and ML 🤖", "Password123!"),
            CreateUser("diana", "diana@example.com", "Diana Prince", "Product manager | Building products people love ❤️", "Password123!"),
            CreateUser("eve", "eve@example.com", "Eve Wilson", "Frontend developer | React enthusiast ⚛️", "Password123!"),
            CreateUser("frank", "frank@example.com", "Frank Miller", "Backend engineer | API design geek 🔧", "Password123!"),
            CreateUser("grace", "grace@example.com", "Grace Hopper", "Computer scientist | Debugging expert 🐛", "Password123!"),
            CreateUser("henry", "henry@example.com", "Henry Ford", "Entrepreneur | Innovation advocate 💡", "Password123!"),
            CreateUser("isabel", "isabel@example.com", "Isabel Chen", "UX/UI Designer | Creating delightful experiences ✨", "Password123!"),
            CreateUser("jack", "jack@example.com", "Jack Smith", "DevOps engineer | Cloud infrastructure specialist ☁️", "Password123!"),
        };

        await _context.Set<User>().AddRangeAsync(users);
        await _context.SaveChangesAsync();
        Console.WriteLine($"Created {users.Count} users");

        // Create tweets
        var tweets = new List<Tweet>
        {
            // John's tweets
            CreateTweet(users[0], "Hello Twitter! Excited to share my journey in software development. #coding #tech"),
            CreateTweet(users[0], "Just deployed my first microservice to production. Feeling accomplished! 🚀 #devops"),
            CreateTweet(users[0], "Working on a new feature using #React and loving it!"),
            
            // Jane's tweets
            CreateTweet(users[1], "Coffee + Code = Perfect morning ☕💻 #developerlife"),
            CreateTweet(users[1], "Excited to announce that I'll be speaking at the tech conference next month! #publicspeaking"),
            CreateTweet(users[1], "Just finished reading 'Clean Code'. Highly recommend it! 📚 #programming"),
            
            // Alice's tweets
            CreateTweet(users[2], "Design systems are the future of scalable UI development #design #ux"),
            CreateTweet(users[2], "New blog post: How to create accessible web applications 🌐"),
            CreateTweet(users[2], "Typography matters more than you think! #webdesign"),
            
            // Bob's tweets
            CreateTweet(users[3], "Contributing to open source is one of the best ways to learn #opensource"),
            CreateTweet(users[3], "Just merged my first PR to a major OSS project! 🎉"),
            CreateTweet(users[3], "Docker has changed the way I develop applications forever #docker #containers"),
            
            // Charlie's tweets
            CreateTweet(users[4], "Machine learning models are only as good as the data you feed them #ai #ml"),
            CreateTweet(users[4], "Working on a cool NLP project using transformers 🤖 #deeplearning"),
            CreateTweet(users[4], "Data visualization is an art and a science #datascience"),
            
            // Diana's tweets
            CreateTweet(users[5], "Product management is all about empathy and understanding user needs ❤️"),
            CreateTweet(users[5], "Just launched our new feature! So proud of the team 🚀"),
            CreateTweet(users[5], "Roadmap planning session today. Exciting things ahead! #product"),
            
            // Eve's tweets
            CreateTweet(users[6], "React hooks have revolutionized functional components ⚛️ #react"),
            CreateTweet(users[6], "CSS Grid + Flexbox = Layout superpowers 💪 #css"),
            CreateTweet(users[6], "Performance optimization is a never-ending journey #webperf"),
            
            // Frank's tweets
            CreateTweet(users[7], "RESTful API design principles everyone should know 🔧 #api"),
            CreateTweet(users[7], "GraphQL vs REST: both have their place in modern architecture"),
            CreateTweet(users[7], "Database indexing can make or break your application performance #database"),
            
            // Grace's tweets
            CreateTweet(users[8], "Debugging is like being a detective in a crime movie 🔍 #debugging"),
            CreateTweet(users[8], "Write tests. Your future self will thank you! #testing"),
            CreateTweet(users[8], "Legacy code is not the enemy, it's the teacher #programming"),
            
            // Henry's tweets
            CreateTweet(users[9], "Innovation happens at the intersection of technology and business 💡"),
            CreateTweet(users[9], "Fail fast, learn faster #startup #entrepreneurship"),
            CreateTweet(users[9], "The best products solve real problems for real people"),
            
            // Isabel's tweets
            CreateTweet(users[10], "User research is the foundation of great design ✨ #ux"),
            CreateTweet(users[10], "Prototyping helps us fail early and iterate quickly #design"),
            CreateTweet(users[10], "Accessibility is not optional, it's essential #a11y"),
            
            // Jack's tweets
            CreateTweet(users[11], "Infrastructure as code has transformed DevOps ☁️ #terraform"),
            CreateTweet(users[11], "Kubernetes orchestration can be complex, but it's worth it #k8s"),
            CreateTweet(users[11], "Monitoring and observability are critical for reliable systems #devops"),
        };

        await _context.Set<Tweet>().AddRangeAsync(tweets);
        await _context.SaveChangesAsync();
        Console.WriteLine($"Created {tweets.Count} tweets");

        // Create follows (create a network of connections)
        var follows = new List<Follow>
        {
            // John follows everyone
            CreateFollow(users[0], users[1]),
            CreateFollow(users[0], users[2]),
            CreateFollow(users[0], users[3]),
            CreateFollow(users[0], users[4]),
            
            // Jane follows tech people
            CreateFollow(users[1], users[0]),
            CreateFollow(users[1], users[2]),
            CreateFollow(users[1], users[6]),
            CreateFollow(users[1], users[7]),
            
            // Alice follows designers and developers
            CreateFollow(users[2], users[0]),
            CreateFollow(users[2], users[1]),
            CreateFollow(users[2], users[6]),
            CreateFollow(users[2], users[10]),
            
            // Bob follows open source enthusiasts
            CreateFollow(users[3], users[0]),
            CreateFollow(users[3], users[7]),
            CreateFollow(users[3], users[8]),
            CreateFollow(users[3], users[11]),
            
            // Charlie follows AI/ML people
            CreateFollow(users[4], users[0]),
            CreateFollow(users[4], users[8]),
            CreateFollow(users[4], users[9]),
            
            // Diana follows product and business people
            CreateFollow(users[5], users[0]),
            CreateFollow(users[5], users[1]),
            CreateFollow(users[5], users[9]),
            CreateFollow(users[5], users[10]),
            
            // Eve follows frontend developers
            CreateFollow(users[6], users[2]),
            CreateFollow(users[6], users[10]),
            CreateFollow(users[6], users[1]),
            
            // Frank follows backend developers
            CreateFollow(users[7], users[3]),
            CreateFollow(users[7], users[8]),
            CreateFollow(users[7], users[11]),
            
            // Grace follows everyone (veteran developer)
            CreateFollow(users[8], users[0]),
            CreateFollow(users[8], users[3]),
            CreateFollow(users[8], users[7]),
            
            // Henry follows innovators
            CreateFollow(users[9], users[0]),
            CreateFollow(users[9], users[4]),
            CreateFollow(users[9], users[5]),
            
            // Isabel follows designers
            CreateFollow(users[10], users[2]),
            CreateFollow(users[10], users[5]),
            CreateFollow(users[10], users[6]),
            
            // Jack follows DevOps people
            CreateFollow(users[11], users[3]),
            CreateFollow(users[11], users[7]),
            CreateFollow(users[11], users[8]),
        };

        await _context.Set<Follow>().AddRangeAsync(follows);
        await _context.SaveChangesAsync();
        Console.WriteLine($"Created {follows.Count} follow relationships");

        // Create likes (users liking various tweets)
        var likes = new List<Like>
        {
            // John likes tech tweets
            CreateLike(users[0], tweets[3]), // Jane's coffee tweet
            CreateLike(users[0], tweets[6]), // Alice's design systems
            CreateLike(users[0], tweets[18]), // Eve's React hooks
            
            // Jane likes multiple tweets
            CreateLike(users[1], tweets[0]), // John's hello
            CreateLike(users[1], tweets[9]), // Bob's open source
            CreateLike(users[1], tweets[21]), // Frank's API design
            
            // Alice likes design-related
            CreateLike(users[2], tweets[4]), // Jane's speaking
            CreateLike(users[2], tweets[30]), // Isabel's user research
            CreateLike(users[2], tweets[19]), // Eve's CSS
            
            // Bob likes development tweets
            CreateLike(users[3], tweets[1]), // John's microservice
            CreateLike(users[3], tweets[11]), // Bob's own Docker tweet
            CreateLike(users[3], tweets[24]), // Grace's debugging
            
            // Charlie likes AI/ML content
            CreateLike(users[4], tweets[12]), // Charlie's own ML tweet
            CreateLike(users[4], tweets[26]), // Grace's testing
            CreateLike(users[4], tweets[14]), // Charlie's data viz
            
            // Diana likes product tweets
            CreateLike(users[5], tweets[15]), // Diana's own empathy tweet
            CreateLike(users[5], tweets[28]), // Henry's innovation
            CreateLike(users[5], tweets[31]), // Isabel's prototyping
            
            // Eve likes frontend
            CreateLike(users[6], tweets[2]), // John's React
            CreateLike(users[6], tweets[7]), // Alice's blog post
            CreateLike(users[6], tweets[19]), // Eve's own CSS tweet
            
            // Frank likes backend
            CreateLike(users[7], tweets[10]), // Bob's PR merge
            CreateLike(users[7], tweets[22]), // Frank's own GraphQL tweet
            CreateLike(users[7], tweets[33]), // Jack's K8s
            
            // Grace likes everything quality
            CreateLike(users[8], tweets[5]), // Jane's Clean Code
            CreateLike(users[8], tweets[25]), // Grace's own tests tweet
            CreateLike(users[8], tweets[32]), // Isabel's accessibility
            
            // Henry likes innovation
            CreateLike(users[9], tweets[16]), // Diana's launch
            CreateLike(users[9], tweets[28]), // Henry's own innovation tweet
            CreateLike(users[9], tweets[29]), // Henry's fail fast
            
            // Isabel likes design
            CreateLike(users[10], tweets[6]), // Alice's design systems
            CreateLike(users[10], tweets[8]), // Alice's typography
            CreateLike(users[10], tweets[30]), // Isabel's own user research
            
            // Jack likes DevOps
            CreateLike(users[11], tweets[1]), // John's deployment
            CreateLike(users[11], tweets[11]), // Bob's Docker
            CreateLike(users[11], tweets[34]), // Jack's own monitoring
        };

        await _context.Set<Like>().AddRangeAsync(likes);
        await _context.SaveChangesAsync();
        Console.WriteLine($"Created {likes.Count} likes");

        Console.WriteLine("✅ Database seeding completed successfully!");
        Console.WriteLine("\nTest Credentials:");
        Console.WriteLine("==================");
        Console.WriteLine("Username: johndoe | Password: Password123!");
        Console.WriteLine("Username: janedoe | Password: Password123!");
        Console.WriteLine("Username: alice   | Password: Password123!");
        Console.WriteLine("(All users have the same password: Password123!)");
    }

    private User CreateUser(string username, string email, string displayName, string bio, string password)
    {
        var user = new User(username, email, displayName);
        
        // Use reflection to set bio (internal setter)
        var bioProperty = typeof(User).GetProperty("Bio");
        bioProperty?.SetValue(user, bio);
        
        // Hash password
        var hashedPassword = _passwordHasher.HashPassword(user, password);
        
        // Use reflection to set password hash (internal setter)
        var passwordProperty = typeof(User).GetProperty("PasswordHash");
        passwordProperty?.SetValue(user, hashedPassword);
        
        return user;
    }

    private Tweet CreateTweet(User user, string content)
    {
        return new Tweet(user.Id, content);
    }

    private Follow CreateFollow(User follower, User followed)
    {
        return new Follow(follower.Id, followed.Id);
    }

    private Like CreateLike(User user, Tweet tweet)
    {
        return new Like(user.Id, tweet.Id);
    }
}
