﻿using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Statiq.Lumen.Extensions;
using Kentico.Kontent.Statiq.Lumen.Models;
using Kentico.Kontent.Statiq.Lumen.Models.ViewModels;
using Kontent.Statiq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Razor;
using System.Linq;

namespace Kentico.Kontent.Statiq.Lumen.Pipelines
{
    public class Home : Pipeline
    {
        public Home(IDeliveryClient deliveryClient)
        {
            Dependencies.AddRange(nameof(Posts), nameof(MenuItems), nameof(Contacts), nameof(Authors), nameof(SiteMetadatas));
            ProcessModules = new ModuleList(
                // pull documents from other pipelines
                new ReplaceDocuments(nameof(Posts)),
                new PaginateDocuments(9),
                new SetDestination(Config.FromDocument((doc, ctx) => Filename(doc))),
                new MergeContent(new ReadFiles("Index.cshtml")),
                new RenderRazor()
                    .WithModel(Config.FromDocument((document, context) =>
                    {
                        var model = new HomeViewModel(document.AsPagedKontent<Article>(),
                           new SidebarViewModel(
                               context.Outputs.FromPipeline(nameof(Contacts)).Select(x => x.AsKontent<Contact>()),
                               context.Outputs.FromPipeline(nameof(MenuItems)).Select(x => x.AsKontent<Menu>()).FirstOrDefault(),
                               context.Outputs.FromPipeline(nameof(Authors)).Select(x => x.AsKontent<Author>()).FirstOrDefault(),
                               context.Outputs.FromPipeline(nameof(SiteMetadatas)).Select(x => x.AsKontent<SiteMetadata>()).FirstOrDefault(),
                               true, string.Empty));
                        return model;
                    }
                    )),
                new KontentImageProcessor()
            );

            OutputModules = new ModuleList {
                new WriteFiles(),
            };
        }

        private static NormalizedPath Filename(IDocument document)
        {
            var index = document.GetInt(Keys.Index);

            return new NormalizedPath($"index{(index > 1 ? index.ToString() : "")}.html");
        }
    }
}
