// Branded - Branded types for C#
// Copyright (C) 2025, 2026 Antoine Aubry

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Branded.Integrations.Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Branded.Showcase.Microsoft.EntityFrameworkCore;

public static class Program
{
    public static void Main()
    {
        using (var db = new ShowcaseDbContext())
        {
            foreach (var existing in db.Widgets)
            {
                db.Widgets.Remove(existing);
            }
            db.SaveChanges();
        }

        using (var db = new ShowcaseDbContext())
        {
            db.Widgets.Add(new Widget
            {
                Id = new(1),
                Owner = "alex"
            });

            db.Widgets.Add(new Widget
            {
                Id = new(2),
                Owner = "alex"
            });

            db.Widgets.Add(new Widget
            {
                Id = new(3),
                Owner = "rain"
            });

            db.SaveChanges();
        }

        using (var db = new ShowcaseDbContext())
        {
            var id = new WidgetIdentifier(2);

            var results = db.Widgets
                .Where(w => w.Id >= id);

            foreach (var result in results)
            {
                Console.WriteLine($"Id: {result.Id}, Owner: {result.Owner}");
            }
        }
    }
}

public class ShowcaseDbContext : DbContext
{
    public DbSet<Widget> Widgets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data source=localhost; Integrated Security=SSPI; Initial Catalog=Sandbox; TrustServerCertificate=true");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new IdentifierConverterConvention());
    }
}

public readonly partial record struct WidgetIdentifier(int Id);

public class Widget
{
    public required WidgetIdentifier Id { get; set; }
    public required string Owner { get; set; }
}
