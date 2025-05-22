using ClientPortalAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClientPortalAPI.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            // Seed Roles
            var adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Seed Admin User
            var adminEmail = "admin@clientportal.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {                adminUser = new ApplicationUser
                {
                    UserName = adminEmail, // Use email as username for consistency
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "Admin@123456"); // Use the same password as in appsettings.json
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }

            // Seed Field Types
            if (!context.FieldTypes.Any())
            {
                context.FieldTypes.AddRange(
                    new FieldType { Name = "Text" },
                    new FieldType { Name = "Number" },
                    new FieldType { Name = "Date" },
                    new FieldType { Name = "Dropdown" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Clients
            var edCorp = new Client { Name = "EdCorp" };
            var emiCorp = new Client { Name = "EmiCorp" };
            var elshaCorp = new Client { Name = "ElshaCorp" };

            if (!context.Clients.Any())
            {
                context.Clients.AddRange(edCorp, emiCorp, elshaCorp);
                await context.SaveChangesAsync();
            }

            // Seed Base Template
            var baseTemplate = new BaseTemplate
            {
                Name = "Base KYC Template",
                Description = "Standard KYC fields"
            };

            if (!context.BaseTemplates.Any())
            {
                context.BaseTemplates.Add(baseTemplate);
                await context.SaveChangesAsync();
            }

            // Seed Form Templates
            var formTemplates = new List<FormTemplate>();

            var edCorpTemplate1 = new FormTemplate
            {
                Name = "EdCorp Onboarding",
                Description = "EdCorp Form 1",
                BaseTemplateId = baseTemplate.Id
            };

            var edCorpTemplate2 = new FormTemplate
            {
                Name = "EdCorp Follow-up",
                Description = "EdCorp Form 2"
            };

            var emiCorpTemplate = new FormTemplate
            {
                Name = "EmiCorp Feedback",
                Description = "Feedback form for EmiCorp"
            };

            var elshaCorpTemplate = new FormTemplate
            {
                Name = "ElshaCorp KYC",
                Description = "Basic KYC",
                BaseTemplateId = baseTemplate.Id
            };

            if (!context.FormTemplates.Any())
            {
                formTemplates.AddRange(new[] { edCorpTemplate1, edCorpTemplate2, emiCorpTemplate, elshaCorpTemplate });
                context.FormTemplates.AddRange(formTemplates);
                await context.SaveChangesAsync();
            }

            // Seed Form Fields
            var textType = context.FieldTypes.First(f => f.Name == "Text");
            var numberType = context.FieldTypes.First(f => f.Name == "Number");

            foreach (var template in formTemplates)
            {
                context.FormFields.AddRange(
                    new FormField
                    {
                        FormTemplateId = template.Id,
                        Label = "Full Name",
                        FieldTypeId = textType.Id,
                        IsRequired = true
                    },
                    new FormField
                    {
                        FormTemplateId = template.Id,
                        Label = "Age",
                        FieldTypeId = numberType.Id,
                        IsRequired = false
                    }
                );
            }

            await context.SaveChangesAsync();

            // Assign templates to clients
            if (!context.FormAssignments.Any())
            {
                context.FormAssignments.AddRange(
                    new FormAssignment { ClientId = edCorp.Id, FormTemplateId = edCorpTemplate1.Id, AssignedAt = DateTime.UtcNow },
                    new FormAssignment { ClientId = edCorp.Id, FormTemplateId = edCorpTemplate2.Id, AssignedAt = DateTime.UtcNow },
                    new FormAssignment { ClientId = emiCorp.Id, FormTemplateId = emiCorpTemplate.Id, AssignedAt = DateTime.UtcNow },
                    new FormAssignment { ClientId = elshaCorp.Id, FormTemplateId = elshaCorpTemplate.Id, AssignedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // Add Sample Submission
            var assignment = context.FormAssignments.First();
            if (!context.Submissions.Any())
            {
                var submission = new Submission
                {
                    FormAssignmentId = assignment.Id,
                    SubmittedByUserId = adminUser.Id,
                    SubmittedAt = DateTime.UtcNow,
                    DataJson = JsonSerializer.Serialize(new
                    {
                        FullName = "John Doe",
                        Age = 30
                    })
                };

                context.Submissions.Add(submission);
                await context.SaveChangesAsync();

                context.ActivityHistories.Add(new ActivityHistory
                {
                    SubmissionId = submission.Id,
                    PerformedByUserId = adminUser.Id,
                    ActionType = "Submit",
                    PerformedAt = DateTime.UtcNow,
                    DataSnapshot = submission.DataJson,
                    Description = "Initial form submission"
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
