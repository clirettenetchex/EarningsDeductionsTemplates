using Microsoft.AspNetCore.Mvc;

namespace EarningDeductionTemplates.Controllers;

[ApiController]
[Route("[controller]")]
public class EarningTemplateController : ControllerBase
{
    [HttpPost]
    public ActionResult AddEarningByTemplate([FromQuery] string templateCode, [FromBody] EarningDefaultRequest? earningRequest)
    {
        // Obviously there should be more robust validation everywhere
        var foundMapping = EarningTemplateMapping.Mappings.TryGetValue(templateCode, out var template);
        if (!foundMapping)
        {
            return BadRequest("something something cannot find mapping");
        }

        var earningDefault = earningRequest.ToDomain();

        var earning = template!.Create(earningDefault);
        return Created("uri", earning);
    }
}

public class EarningDefaultRequest
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? PayCycle { get; set; }
}

public static class EarningDefaultRequestExtensions
{
    public static EarningDefault? ToDomain(this EarningDefaultRequest earningDefault)
    {
        if (string.IsNullOrWhiteSpace(earningDefault.Code) && string.IsNullOrWhiteSpace(earningDefault.Description) && string.IsNullOrWhiteSpace(earningDefault.PayCycle))
            return null;

        return new EarningDefault(earningDefault.Code, earningDefault.Description, (PayCycle) Enum.Parse(typeof(PayCycle), earningDefault.PayCycle));
    }
}

public abstract record StubCodeDefault(string Code, string Description, PayCycle PayCycle = PayCycle.OneTwoThreeFourFive);

public record EarningDefault(string Code, string Description, PayCycle PayCycle) : StubCodeDefault(Code, Description, PayCycle);

// This proof of concept doesn't go too deep on Deductions. Maybe see this through as an exercise?
public record DeductionDefault(decimal EmployerMatch, string Code, string Description, PayCycle PayCycle) : StubCodeDefault(Code, Description, PayCycle);

public interface ITemplate<T, TDefault>
    where T : AddableThing
    where TDefault : StubCodeDefault
{
    string Code { get; }
    string Description { get; }
    PayCycle PayCycle { get; }
    T Create(TDefault? _default);
}

public interface IEarningTemplate : ITemplate<Earning, EarningDefault>
{
    CalculationMethod CalculationMethod { get; }
    bool IncludeIn401k { get; }
    bool IncludeInProductiveHours { get; }
    bool IncludeInOvertime { get; }

    // something like this would probably be better to reduce code reuse
    // reuse - I mean the properties here and repeated in the Earning type
    // Earning Earning { get; }
}

public static class EarningTemplateMapping
{
    // could probably add all earnings codes here
    public static string RegularCode = "Regular";
    public static Dictionary<string, IEarningTemplate> Mappings = new()
    {
        { RegularCode, new RegularEarningTemplate() }
    };
}

// would be a good exercise to do another earning template like Overtime, Sick etc.
public sealed class RegularEarningTemplate : IEarningTemplate
{
    public CalculationMethod CalculationMethod => CalculationMethod.FlatAmount;
    public bool IncludeIn401k => true;
    public bool IncludeInProductiveHours => true;
    public bool IncludeInOvertime => true;
    public string Code => EarningTemplateMapping.RegularCode;
    public string Description => "Regular earnings";
    public PayCycle PayCycle => PayCycle.OneTwoThreeFourFive;

    public Earning Create(EarningDefault? earningDefault)
        => earningDefault is null
            ? new Earning(Code, Description, PayCycle, CalculationMethod, IncludeIn401k, IncludeInProductiveHours, IncludeInOvertime)
            : new Earning(earningDefault, CalculationMethod, IncludeIn401k, IncludeInProductiveHours, IncludeInOvertime);
}

public record AddableThing(); // Deduction would be another AddableThing

public sealed record Earning : AddableThing
{
    public Earning(
        string code,
        string description,
        PayCycle payCycle,
        CalculationMethod calculationMethod,
        bool includeIn401k,
        bool includeInProductiveHours,
        bool includeInOvertime)
    {
        Code = code;
        Description = description;
        PayCycle = payCycle;
        CalculationMethod = calculationMethod;
        IncludeIn401k = includeIn401k;
        IncludeInProductiveHours = includeInProductiveHours;
        IncludeInOvertime = includeInOvertime;
    }

    public Earning(
        EarningDefault earningDefault,
        CalculationMethod calculationMethod,
        bool includeIn401k,
        bool includeInProductiveHours,
        bool includeInOvertime
    ) : this(
        code: earningDefault.Code,
        description: earningDefault.Description,
        payCycle: earningDefault.PayCycle,
        calculationMethod: calculationMethod,
        includeIn401k: includeIn401k,
        includeInProductiveHours: includeInProductiveHours,
        includeInOvertime: includeInOvertime) {}

    public string Code { get; }
    public string Description { get; }
    public PayCycle PayCycle { get; }
    public CalculationMethod CalculationMethod { get; }   
    public bool IncludeIn401k { get; }
    public bool IncludeInProductiveHours { get; }
    public bool IncludeInOvertime { get; }
}

public enum PayCycle
{
    One,
    OneTwo,
    OneThree,
    OneFour,
    OneFive,
    OneTwoThree,
    OneTwoFour,
    OneTwoFive,
    OneTwoThreeFour,
    OneTwoThreeFive,
    OneTwoThreeFourFive,
    Two,
    TwoThree,
    TwoFour,
    TwoFive,
    TwoThreeFour,
    TwoThreeFive,
    TwoThreeFourFive,
    Three,
    ThreeFour,
    ThreeFive,
    ThreeFourFive,
    Four,
    FourFive,
    Five
}

public enum CalculationMethod
{
    FlatAmount,
    UnitXRate
}
