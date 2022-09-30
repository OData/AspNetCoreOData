//-----------------------------------------------------------------------------
// <copyright file="SingletonEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Singleton
{
    internal class SingletonEdmModel
    {
        public static IEdmModel GetExplicitModel(string singletonName)
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            // Define EntityType of Partner
            var partner = builder.EntityType<Partner>();
            partner.HasKey(p => p.ID);
            partner.Property(p => p.Name);
            var partnerCompany = partner.HasRequired(p => p.Company);

            // Define Enum Type
            var category = builder.EnumType<CompanyCategory>();
            category.Member(CompanyCategory.IT);
            category.Member(CompanyCategory.Communication);
            category.Member(CompanyCategory.Electronics);
            category.Member(CompanyCategory.Others);

            // Define EntityType of Company
            var company = builder.EntityType<Company>();
            company.HasKey(p => p.ID);
            company.Property(p => p.Name);
            company.Property(p => p.Revenue);
            company.EnumProperty(p => p.Category);
            var companyPartners = company.HasMany(p => p.Partners);
            companyPartners.IsNotCountable();

            var companyBranches = company.CollectionProperty(p => p.Branches);

            // Define Complex Type: Office
            var office = builder.ComplexType<Office>();
            office.Property(p => p.City);
            office.Property(p => p.Address);

            // Define Derived Type: SubCompany
            var subCompany = builder.EntityType<SubCompany>();
            subCompany.DerivesFrom<Company>();
            subCompany.Property(p => p.Location);
            subCompany.Property(p => p.Description);
            subCompany.ComplexProperty(p => p.Office);

            builder.Namespace = typeof(Partner).Namespace;

            // Define PartnerSet and Company(singleton)
            EntitySetConfiguration<Partner> partnersConfiguration = builder.EntitySet<Partner>("Partners");
            //partnersConfiguration.HasIdLink(c=>c.GenerateSelfLink(false), true);
            //partnersConfiguration.HasSingletonBinding(c => c.Company, singletonName);
            //Func<ResourceContext<Partner>, IEdmNavigationProperty, Uri> link = (eic, np) => eic.GenerateNavigationPropertyLink(np, false);
            //partnersConfiguration.HasNavigationPropertyLink(partnerCompany, link, true);
            partnersConfiguration.EntityType.Collection.Action("ResetDataSource");

            SingletonConfiguration<Company> companyConfiguration = builder.Singleton<Company>("Umbrella");
            //companyConfiguration.HasIdLink(c => c.GenerateSelfLink(false), true);
            //companyConfiguration.HasManyBinding(c => c.Partners, "Partners");
            //Func<ResourceContext<Company>, IEdmNavigationProperty, Uri> linkFactory = (eic, np) => eic.GenerateNavigationPropertyLink(np, false);
            //companyConfiguration.HasNavigationPropertyLink(companyPartners, linkFactory, true);
            companyConfiguration.EntityType.Action("ResetDataSource");
            companyConfiguration.EntityType.Function("GetPartnersCount").Returns<int>();

            SingletonConfiguration<Company> monstersIncConfiguration = builder.Singleton<Company>("MonstersInc");
            monstersIncConfiguration.EntityType.Function("GetPartnersCount").Returns<int>();

            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel(string singletonName)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<Partner> partnersConfiguration = builder.EntitySet<Partner>("Partners");
            EntityTypeConfiguration<Partner> partnerConfiguration = partnersConfiguration.EntityType;
            partnerConfiguration.Collection.Action("ResetDataSource");

            SingletonConfiguration<Company> companyConfiguration = builder.Singleton<Company>(singletonName);
            companyConfiguration.EntityType.Action("ResetDataSource");
            companyConfiguration.EntityType.Function("GetPartnersCount").Returns<int>();

            builder.Namespace = typeof(Company).Namespace;

            return builder.GetEdmModel();
        }

        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<Partner> partnersConfiguration = builder.EntitySet<Partner>("Partners");
            EntityTypeConfiguration<Partner> partnerConfiguration = partnersConfiguration.EntityType;
            partnerConfiguration.Collection.Action("ResetDataSource");

            // Singleton "Umbrella"
            SingletonConfiguration<Company> umbrellaConfiguration = builder.Singleton<Company>("Umbrella");
            //umbrellaConfiguration.EntityType.Action("ResetDataSource");
            umbrellaConfiguration.EntityType.Function("GetPartnersCount").Returns<int>();

            // Singleton "MonstersInc"
            SingletonConfiguration<Company> monstersIncConfiguration = builder.Singleton<Company>("MonstersInc");
            //monstersIncConfiguration.EntityType.Action("ResetDataSource");
            monstersIncConfiguration.EntityType.Function("GetPartnersCount").Returns<int>();
            builder.EntityType<Project>();

            // Singleton "Sample"
            builder.Singleton<Sample>("Sample");
            builder.EntityType<SampleItems>();

            builder.Namespace = typeof(Company).Namespace;
            return builder.GetEdmModel();
        }
    }
}
