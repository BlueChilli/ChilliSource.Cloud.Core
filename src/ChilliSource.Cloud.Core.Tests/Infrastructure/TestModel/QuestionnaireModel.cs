using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Tests.Infrastructure.TestModel
{
    public class QuestionnaireApiModel
    {
        public int Id { get; set; }

        public string Topic { get; set; }

        public SubTypeApiModel PostSubType { get; set; }

        public TagApiModel PublishedLocation { get; set; }

        public TagApiModel PostLength { get; set; }

        public TagApiModel EmotionalResponse { get; set; }

        public TagApiModel BrandPositioning { get; set; }

        public TagApiModel TimeSensitivity { get; set; }

        public QuestionnaireStatus Status { get; set; }

        public DateTime? LastViewedAt { get; set; }
        public DateTime? RecipeLinkedAt { get; set; }

        public string ViewUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string DownloadDocUrl { get; set; }

        public string RecipeFilePath { get; set; }
        public string RecipeFileDocPath { get; set; }
    }

    public enum TagType
    {
        None = 0,
        PostSubType = 1,
        IntroType,
        PublishedLocation,
        PostLength,
        EmotionalResponse,
        BrandPositioning,
        TimeSensitivity
    }


    public enum QuestionnaireStatus
    {
        Crafting = 0,
        RecipeLinked
    }

    public class TagApiModel
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public virtual string Description { get; set; }
        public string Comment { get; set; }
    }

    public class SubTypeApiModel : TagApiModel
    {
        public PostTypeSummaryApiModel PostType { get; set; }

        public string SubTypeDescription { get; set; }

        public override string Description
        {
            get
            {
                if (PostType == null)
                    return this.SubTypeDescription;

                return $"{this.PostType.Description} - {this.SubTypeDescription}";
            }
            set { }
        }
    }

    public class PostTypeSummaryApiModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
