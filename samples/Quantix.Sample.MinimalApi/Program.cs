// Quantix.Sample.MinimalApi — reference usage of Quantix.
//
// AddQuantix and the QuantixMediator are generated into this project by the Quantix source
// generator. There is no runtime reflection: every endpoint below dispatches through
// generated, strongly-typed code.
//
// The host uses WebApplication.CreateSlimBuilder and the project publishes with Native AOT
// (see the .csproj). Every endpoint returns plain text, so the response path stays
// serializer-free — the sample publishes with zero trim or AOT warnings.

using Quantix;
using Quantix.Sample;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddQuantix();

var app = builder.Build();

// Free-text route values are validated at the boundary: a value longer than MaxTextLength is
// rejected with 400 Bad Request before any message is sent.
const int MaxTextLength = 64;

// A query — returns a personalised greeting.
app.MapGet(
    "/greeting/{name}",
    async (string name, IMediator mediator, CancellationToken ct) =>
        TooLong(name, nameof(name))
        ?? Results.Text(await mediator.Send(new GetGreeting(name), ct)));

// A command with a result — records a page visit and reports that page's running count.
app.MapGet(
    "/visit/{page}",
    async (string page, IMediator mediator, CancellationToken ct) =>
    {
        if (TooLong(page, nameof(page)) is { } rejected)
        {
            return rejected;
        }

        int count = await mediator.Send(new RecordVisit(page), ct);
        return count == 0
            ? Results.Text(
                $"Visit to '{page}' not recorded: the sample tracks at most {RecordVisitHandler.MaxPages} pages.",
                statusCode: StatusCodes.Status503ServiceUnavailable)
            : Results.Text($"Visit #{count} recorded for '{page}'.");
    });

// A generic query — DescribeEntity<Product> is closed and dispatched at compile time.
app.MapGet(
    "/describe/{id:int}",
    (int id, IMediator mediator, CancellationToken ct) => mediator.Send(new DescribeEntity<Product>(id), ct));

app.Run();

// Returns a 400 response naming the field when the value exceeds MaxTextLength; otherwise null.
static IResult? TooLong(string value, string field) =>
    value.Length > MaxTextLength
        ? Results.Text(
            $"'{field}' must be at most {MaxTextLength} characters.",
            statusCode: StatusCodes.Status400BadRequest)
        : null;
