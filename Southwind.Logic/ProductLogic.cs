﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Engine.Maps;
using Signum.Engine.DynamicQuery;
using System.Reflection;
using Southwind.Entities;
using Signum.Engine;
using Signum.Utilities;
using Signum.Entities;
using Signum.Engine.Operations;

namespace Southwind.Logic
{
    public static class ProductLogic
    {
        public static ResetLazy<Dictionary<CategoryDN, List<ProductDN>>> ActiveProducts;

        public static void Start(SchemaBuilder sb, DynamicQueryManager dqm)
        {
            if (sb.NotDefined(MethodInfo.GetCurrentMethod()))
            {
                sb.Include<ProductDN>();

                ActiveProducts = sb.GlobalLazy(() =>
                    Database.Query<ProductDN>()
                    .Where(a => !a.Discontinued)
                    .Select(p => new { Category = p.Category.Entity, Product = p })
                    .GroupToDictionary(a => a.Category, a => a.Product),
                    new InvalidateWith(typeof(ProductDN)));

                dqm.RegisterQuery(typeof(ProductDN), () =>
                    from p in Database.Query<ProductDN>()
                    select new
                    {
                        Entity = p.ToLite(),
                        p.Id,
                        p.ProductName,
                        p.Supplier,
                        p.Category,
                        p.QuantityPerUnit,
                        p.UnitPrice,
                        p.UnitsInStock,
                        p.Discontinued
                    });

                dqm.RegisterQuery(ProductQuery.Current, () =>
                    from p in Database.Query<ProductDN>()
                    where !p.Discontinued
                    select new
                    {
                        Entity = p.ToLite(),
                        p.Id,
                        p.ProductName,
                        p.Supplier,
                        p.Category,
                        p.QuantityPerUnit,
                        p.UnitPrice,
                        p.UnitsInStock,
                    });

                dqm.RegisterQuery(typeof(SupplierDN), () =>
                    from s in Database.Query<SupplierDN>()
                    select new
                    {
                        Entity = s.ToLite(),
                        s.Id,
                        s.CompanyName,
                        s.ContactName,
                        s.Phone,
                        s.Fax,
                        s.HomePage,
                        s.Address
                    });

                dqm.RegisterQuery(typeof(CategoryDN), () =>
                    from s in Database.Query<CategoryDN>()
                    select new
                    {
                        Entity = s.ToLite(),
                        s.Id,
                        s.CategoryName,
                        s.Description,
                    });



                new Graph<ProductDN>.Execute(ProductOperation.Save)
                {
                    AllowsNew = true,
                    Lite = false,
                    Execute = (e, _) => { }
                }.Register();

                new Graph<SupplierDN>.Execute(SupplierOperation.Save)
                {
                    AllowsNew = true,
                    Lite = false,
                    Execute = (e, _) => { }
                }.Register();

                new Graph<CategoryDN>.Execute(CategoryOperation.Save)
                {
                    AllowsNew = true,
                    Lite = false,
                    Execute = (e, _) => { }
                }.Register();
            }
        }
    }
}
