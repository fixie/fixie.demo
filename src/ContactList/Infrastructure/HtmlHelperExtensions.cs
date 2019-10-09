namespace ContactList.Infrastructure
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public static class HtmlHelperExtensions
    {
        public static TagBuilder Input<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var div = new TagBuilder("div");
            div.AddCssClass("form-group");

            div.InnerHtml.AppendHtml(html.LabelFor(expression));
            div.InnerHtml.Append(":");
            div.InnerHtml.AppendLine();

            var htmlAttributes = new { @class = "form-control" };
            div.InnerHtml.AppendHtml(
                IsEnumType(typeof(TValue))
                    ? html.DropDownListFor(expression, html.GetEnumSelectList(typeof(TValue)), "-- Select --", htmlAttributes)
                    : html.EditorFor(expression, new { htmlAttributes }));
            div.InnerHtml.AppendLine();

            var validationMessage = html.ValidationMessageFor(expression);
            div.InnerHtml.AppendHtml(validationMessage);
            div.InnerHtml.AppendLine();

            return div;
        }

        static bool IsEnumType(this Type type)
            => type.IsEnum || Nullable.GetUnderlyingType(type)?.IsEnum == true;

        public static TagBuilder SaveButton<TModel>(this IHtmlHelper<TModel> html)
        {
            var button = new TagBuilder("button");

            button.MergeAttribute("type", "submit");
            button.AddCssClass("btn");
            button.AddCssClass("btn-primary");
            button.InnerHtml.Append("Save");

            return button;
        }

        public static TagBuilder CancelButton<TModel>(this IHtmlHelper<TModel> html, string url)
        {
            var button = new TagBuilder("a");

            button.MergeAttribute("href", url);
            button.MergeAttribute("role", "button");
            button.AddCssClass("btn");
            button.AddCssClass("btn-secondary");
            button.InnerHtml.Append("Cancel");

            return button;
        }

        public static TagBuilder Button<TModel>(this IHtmlHelper<TModel> html, string text, string url)
        {
            var button = new TagBuilder("a");

            button.MergeAttribute("href", url);
            button.MergeAttribute("role", "button");
            button.AddCssClass("btn");
            button.AddCssClass("btn-primary");
            button.InnerHtml.Append(text);

            return button;
        }

        public static TagBuilder EditButton<TModel>(this IHtmlHelper<TModel> html, string url)
        {
            var button = new TagBuilder("a");

            button.MergeAttribute("href", url);
            button.InnerHtml.AppendHtml(MaterialIcon("edit"));

            return button;
        }

        public static TagBuilder ConfirmationRequiredButton<TModel>(this IHtmlHelper<TModel> html,
            string url,
            string confirmationTitle,
            string confirmationPrompt,
            string confirmationText,
            string iconName)
        {
            var button = new TagBuilder("a");

            button.MergeAttribute("href", "#");
            button.MergeAttribute("role", "button");
            button.MergeAttribute("data-toggle", "modal");
            button.MergeAttribute("data-target", "#confirmationModal");
            button.MergeAttribute("data-url", url);
            button.MergeAttribute("data-confirmation-title", confirmationTitle);
            button.MergeAttribute("data-confirmation-prompt", confirmationPrompt);
            button.MergeAttribute("data-confirmation-text", confirmationText);
            button.InnerHtml.AppendHtml(MaterialIcon(iconName));

            return button;
        }

        public static TagBuilder DeleteButton<TModel>(this IHtmlHelper<TModel> html, string url, string itemDescription)
        {
            return html.ConfirmationRequiredButton(url,
                confirmationTitle: "Confirm Deletion",
                confirmationPrompt: $"Are you sure you want to delete {itemDescription}? This operation cannot be undone.",
                confirmationText: "Delete",
                iconName: "delete");
        }

        static TagBuilder MaterialIcon(string iconName)
        {
            var icon = new TagBuilder("i");

            icon.AddCssClass("material-icons");
            icon.InnerHtml.Append(iconName);

            return icon;
        }
    }
}