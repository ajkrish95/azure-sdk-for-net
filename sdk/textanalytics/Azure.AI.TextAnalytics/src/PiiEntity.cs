﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.AI.TextAnalytics
{
    /// <summary>
    /// A word or phrase identified as a Personally Identifiable Information
    /// that can be categorized as known type in a given taxonomy.
    /// The set of categories recognized by the text analytics service is described at
    /// <a href="https://docs.microsoft.com/en-us/azure/cognitive-services/Text-Analytics/named-entity-types?tabs=personal"/>.
    /// </summary>
    public readonly struct PiiEntity
    {
        internal PiiEntity(string text, string category, string subCategory, int offset, int length, double score)
        {
            Text = text;
            Category = category;
            SubCategory = subCategory;
            Offset = offset;
            Length = length;
            Score = score;
        }

        /// <summary>
        /// Gets the entity text as it appears in the input document.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the entity category inferred by the text analytics service's
        /// named entity recognition model.  The list of available categories is
        /// described at
        /// <a href="https://docs.microsoft.com/en-us/azure/cognitive-services/Text-Analytics/named-entity-types"/>.
        /// </summary>
        public EntityCategory Category { get; }

        /// <summary>
        /// Gets the sub category of the entity inferred by the text analytics service's
        /// named entity recognition model.  This property may not have a value if
        /// a sub category doesn't exist for this entity.  The list of available categories and
        /// subcategories is described at
        /// <a href="https://docs.microsoft.com/en-us/azure/cognitive-services/Text-Analytics/named-entity-types"/>.
        /// </summary>
        public EntitySubCategory SubCategory { get; }

        /// <summary>
        /// Gets the start position for the matching text in the input document.
        /// The offset unit is unicode character count.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Gets the length of the matching text in the input document.
        /// The length unit is unicode character count.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets a score between 0 and 1, indicating the confidence that the
        /// text substring matches this inferred entity.
        /// </summary>
        public double Score { get; }
    }
}
