using ebooks_dotnet7_api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("ebooks"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var ebooks = app.MapGroup("api/ebook");

// TODO: Add more routes
ebooks.MapPost("/", CreateEBookAsync);
ebooks.MapGet("/", GetEBooks);
ebooks.MapPut("/{id}", UpdateEBookAsync);
ebooks.MapPut("/{id}/change-availability", ChangeAvailabilityAsync);
ebooks.MapPut("/{id}/increment-stock", ChangeStockAsync);
ebooks.MapPut("/purchase", PurchaseEBookAsync);
ebooks.MapDelete("/{id}", DeleteEBookAsync);

app.Run();

static async Task<IResult> CreateEBookAsync([FromBody] CreateEBookDto createDto, DataContext db)
{
    var existingEbook = db.EBooks.Where(e => e.Title == createDto.Title && e.Author == createDto.Author).FirstOrDefault();
    if(existingEbook==null)
    {
        var newBook= new EBook
        {
            Title = createDto.Title,
            Author = createDto.Author,
            Genre = createDto.Genre,
            Format = createDto.Format,
            Price = createDto.Price,
            IsAvailable = true,
            Stock = 0
        };

        db.EBooks.Add(newBook);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/ebook/{newBook.Id}", newBook); 
    }
    return TypedResults.BadRequest("El libro ya existe!");
}

static IResult GetEBooks(string? genre, string? author, string? format, DataContext db)
{
    IQueryable<EBook> query = db.EBooks;
    if(genre != string.Empty)
    {
        query = query.Where(c => c.Genre == genre);
    }
    if(author != string.Empty)
    {
        query=db.EBooks.Where(c => c.Genre == author);
    }
    if(format != string.Empty)
    {
        query=db.EBooks.Where(c => c.Genre == format);
    }
    List<EBook> list = query.OrderBy(e => e.Title).ToList();
    return TypedResults.Ok(list);
}

static async Task<IResult> UpdateEBookAsync(int id, [FromBody] UpdateEBookDto updateDto, DataContext db)
{
    var existingEbook = await db.EBooks.Where(e => e.Id == id).FirstOrDefaultAsync();
    if(existingEbook != null)
    {
        existingEbook.Title = updateDto.Title;
        existingEbook.Author = updateDto.Author;
        existingEbook.Genre = updateDto.Genre;
        existingEbook.Format = updateDto.Format;
        existingEbook.Price = updateDto.Price;

        db.EBooks.Entry(existingEbook).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return TypedResults.Ok(existingEbook);
    }
    return TypedResults.NotFound("No se ha encontrado un libro que actualizar!");
}

static async Task<IResult> ChangeAvailabilityAsync(int id, DataContext db)
{
    var existingEbook = await db.EBooks.Where(e => e.Id == id).FirstOrDefaultAsync();
    if(existingEbook != null)
    {
        existingEbook.IsAvailable = !existingEbook.IsAvailable;

        db.EBooks.Entry(existingEbook).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return TypedResults.Ok(existingEbook);
    }
    return TypedResults.NotFound("No se ha encontrado un libro que actualizar!");
}

static async Task<IResult> ChangeStockAsync(int id, [FromBody] ChangeStockDto changeStockDto, DataContext db)
{
    var existingEbook = await db.EBooks.Where(e => e.Id == id).FirstOrDefaultAsync();
    if(existingEbook != null)
    {
        existingEbook.Stock += changeStockDto.Stock;

        db.EBooks.Entry(existingEbook).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return TypedResults.Ok(existingEbook);
    }
    return TypedResults.NotFound("No se ha encontrado un libro que actualizar!");
}

static async Task<IResult> PurchaseEBookAsync([FromBody] PurchaseEBookDto purchaseDto, DataContext db)
{
    var existingEbook = await db.EBooks.Where(e => e.Id == purchaseDto.Id).FirstOrDefaultAsync();
    if(existingEbook != null)
    {
        if(existingEbook.Stock < purchaseDto.Amount)
        {
            return TypedResults.BadRequest("La cantidad deseada excede el stock disponible!");
        }
        var totalPrice = existingEbook.Price*existingEbook.Stock;
        if(totalPrice < purchaseDto.Amount)
        {
            return TypedResults.BadRequest("El monto a pagar es insuficiente!");
        }
        existingEbook.Stock -= purchaseDto.Amount;
        db.EBooks.Entry(existingEbook).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return TypedResults.Ok(existingEbook);
    }
    return TypedResults.NotFound("No se ha encontrado un libro para comprar!");
}

static async Task<IResult> DeleteEBookAsync(int id, DataContext db)
{
    var existingEbook = await db.EBooks.Where(e => e.Id == id).FirstOrDefaultAsync();
    if(existingEbook != null)
    {
        db.Remove(existingEbook);
        await db.SaveChangesAsync();
        return TypedResults.Ok("Libro eliminado correctamente.");
    }
    return TypedResults.NotFound("No se ha encontrado el libro a eliminar!");
}