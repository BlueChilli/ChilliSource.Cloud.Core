using ChilliSource.Cloud.Core.Tests.Infrastructure.TestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Tests.Infrastructure.TestData
{
    public class Questionnaire
    {
        public int Id { get; set; }

        public string Topic { get; set; }

        public int PostSubTypeId { get; set; }
        public PostSubType PostSubType { get; set; }

        public int PublishedLocationId { get; set; }
        public PublishedLocation PublishedLocation { get; set; }

        public int PostLengthId { get; set; }
        public PostLength PostLength { get; set; }

        public int EmotionalResponseId { get; set; }
        public EmotionalResponse EmotionalResponse { get; set; }

        public int BrandPositioningId { get; set; }
        public BrandPositioning BrandPositioning { get; set; }

        public int TimeSensitivityId { get; set; }
        public TimeSensitivity TimeSensitivity { get; set; }

        public DateTime CreatedAt { get; set; }

        public QuestionnaireStatus Status { get; set; }

        public DateTime? LastViewedAt { get; set; }
        public DateTime? RecipeLinkedAt { get; set; }

        public string RecipeFilePath { get; set; }

        public string RecipeFileDocPath { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }

        string _tagName;

        public string TagName { get { return _tagName; } set { _tagName = value?.Trim().ToUpper(); } }

        public string Description { get; set; }

        public string Comment { get; set; }

        //This may used to force a specific order, if this is Zero Description will be used instead.
        public int Order { get; set; }

        public bool IsDisabled { get; set; }

        public virtual TagType GetTagType() { return TagType.None; }
    }

    public class PublishedLocation : Tag
    {
        public override TagType GetTagType() { return TagType.PublishedLocation; }
    }

    public class IntroType : Tag
    {
        public override TagType GetTagType() { return TagType.IntroType; }
    }

    public class PostLength : Tag
    {
        public override TagType GetTagType() { return TagType.PostLength; }
    }

    public class PostSubType : Tag
    {
        public int? PostTypeId { get; set; }
        public PostType PostType { get; set; }
        
        public string SubTypeDescription { get; internal set; }

        public override TagType GetTagType() { return TagType.PostSubType; }
    }

    public class EmotionalResponse : Tag
    {
        public override TagType GetTagType() { return TagType.EmotionalResponse; }
    }

    public class BrandPositioning : Tag
    {
        public override TagType GetTagType() { return TagType.BrandPositioning; }
    }
    public class TimeSensitivity : Tag
    {
        public override TagType GetTagType() { return TagType.TimeSensitivity; }
    }

    public class PostType
    {
        public int Id { get; set; }

        string _description;

        public string Description { get { return _description; } set { _description = value?.Trim(); } }

        public string Comment { get; set; }

        public bool IsDisabled { get; set; }
    }
}
