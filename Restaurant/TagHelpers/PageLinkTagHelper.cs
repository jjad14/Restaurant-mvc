using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Restaurant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.TagHelpers
{
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PageLinkTagHelper : TagHelper
    {
        private IUrlHelperFactory _urlHelperFactory;

        public PageLinkTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        // view context object is the object that provides access to things like HTTPContext, HttpRequest, HttpResponse
        [ViewContext]
        [HtmlAttributeNotBound] // basically says that this attribute isn't one that you intend to set via a tag helper attribute in Html
        public ViewContext ViewContext { get; set; }

        public PagingInfo PageModel { get; set; }
        public string PageAction { get; set; }
        public bool PageClassesEnabled { get; set; }
        public string PageClass { get; set; }
        public string PageClassNormal { get; set; }
        public string PageClassSelected { get; set; }

        // In this particular tag helper. We will not be using context but we would be using the tag helper output 
        // and we will modify this output to append the html and add pagination
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // So the first thing we will use is that you were a helper.
            // So based on that we get the url helper object right here.
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            // We will add a tag builder in which we have the building our custom tag helper.
            TagBuilder result = new TagBuilder("div");

            // Then what we want to do is inside page model, We have total number of page
            // So if you go to page model if you have 15 items and 5 items per page the total page is 3
            // and in our pagination we want to display three separate numbers like 1 2 and 3.
            // So for each we will be creating 1 separate pagination
            for (int i = 1; i <= PageModel.totalPage; i++)
            {
                TagBuilder tag = new TagBuilder("a");
                // what we want to do is we want to replace a colon with what is the current page
                string url = PageModel.urlParam.Replace(":", i.ToString());
                // if anyone clicks on the page number one it redirects to the page with first content of data and so on
                tag.Attributes["href"] = url;

                // assign css classes
                if (PageClassesEnabled)
                {
                    tag.AddCssClass(PageClass);
                    // we should add the current page classes only to the selected page.
                    tag.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
                }

                // content of our page nation will be i which is numbers 1 2 3 and so on
                tag.InnerHtml.Append(i.ToString());
                // result is our parent div, append our anchor tags to the div
                result.InnerHtml.AppendHtml(tag);
            }

            // Once that is appended to our main div we want to output it and for that we'll use the tag helper output
            output.Content.AppendHtml(result.InnerHtml);
        }


    }
}
