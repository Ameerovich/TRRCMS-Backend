using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Persons.Commands.CreatePerson;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.Persons.Queries.GetAllPersons;
using TRRCMS.Application.Persons.Queries.GetPerson;

namespace TRRCMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PersonsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new person
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PersonDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PersonDto>> CreatePerson([FromBody] CreatePersonCommand command)
        {
            try
            {
                var person = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, person);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get person by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PersonDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersonDto>> GetPerson(Guid id)
        {
            var query = new GetPersonQuery(id);
            var person = await _mediator.Send(query);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        /// <summary>
        /// Get all persons
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<PersonDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PersonDto>>> GetAllPersons()
        {
            var query = new GetAllPersonsQuery();
            var persons = await _mediator.Send(query);
            return Ok(persons);
        }
    }
}