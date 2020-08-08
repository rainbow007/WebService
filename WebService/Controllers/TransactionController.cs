using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebService.Data;
using WebService.Models;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly UserContext _userContext;
        private readonly MovieContext _movieContext;

        public TransactionController(UserContext context, MovieContext movieContext)
        {
            _userContext = context;
            _movieContext = movieContext;
        }

        // GET: api/Transaction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            return await _userContext.User.ToListAsync();
        }

        // GET: api/Transaction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userContext.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Transaction/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _userContext.Entry(user).State = EntityState.Modified;

            try
            {
                await _userContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Transaction
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            //_userContext.User.Add(user);
            //await _userContext.SaveChangesAsync();

            //return CreatedAtAction("GetUser", new { id = user.Id }, user);
            using (IDbContextTransaction transaction1 = _userContext.Database.BeginTransaction())
            {
                try
                {
                    user.Id = user.Id + 1;
                    _userContext.User.Add(user);
                    _userContext.SaveChanges();
                    user.Id = user.Id + 1;
                    user.FirstName = "Second";
                    user.LastName = "LAst1";
                    _userContext.User.Add(user);
                    _userContext.SaveChanges();

                    using (IDbContextTransaction transation2 = _movieContext.Database.BeginTransaction())
                    {
                        try
                        {
                            _movieContext.Movie.Add(new Movie()
                            {
                                Title = "Movie 1-",
                                Genre = "Action"
                            });
                            _movieContext.SaveChanges();

                            // Exception case Genra Required
                            _movieContext.Movie.Add(new Movie()
                            {
                                Title = "Movie 2",
                            });
                            _movieContext.SaveChanges();

                            transation2.Commit();
                        }
                        catch (Exception ex)
                        {
                            transation2.Rollback();
                            Console.WriteLine("Error occurred transaction2.");
                            throw new Exception(ex.ToString());
                        }
                    }


                    transaction1.Commit();
                }
                catch (Exception ex)
                {
                    transaction1.Rollback();
                    Console.WriteLine("Error occurred transaction 1.");
                }
            }
            return NoContent();
        }

        // DELETE: api/Transaction/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var user = await _userContext.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _userContext.User.Remove(user);
            await _userContext.SaveChangesAsync();

            return user;
        }

        private bool UserExists(int id)
        {
            return _userContext.User.Any(e => e.Id == id);
        }
    }
}
