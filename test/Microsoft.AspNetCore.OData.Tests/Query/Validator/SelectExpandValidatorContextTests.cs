//-----------------------------------------------------------------------------
// <copyright file="SelectExpandValidatorContextTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator
{
    public class SelectExpandValidatorContextTests
    {
        [Fact]
        public void Clone_CopiesContext()
        {
            // Arrange
            SelectExpandValidatorContext context = new SelectExpandValidatorContext
            {
                ValidationSettings = new ODataValidationSettings { MaxSelectExpandNodeCount = 7 },
                CurrentDepth = 3,
                RemainingDepth = 2
            };

            // Act
            SelectExpandValidatorContext clone = context.Clone();

            // Assert
            Assert.NotSame(context, clone);
            Assert.Equal(7, clone.ValidationSettings.MaxSelectExpandNodeCount);
            Assert.Equal(3, clone.CurrentDepth);
            Assert.Equal(2, clone.RemainingDepth);
        }

        [Fact]
        public void IncrementSelectExpandNodeCount_AllowsUpToMax_ThenThrows()
        {
            // Arrange
            SelectExpandValidatorContext context = new SelectExpandValidatorContext();
            context.InitializeSelectExpandNodeCount(3);

            // Act & Assert: the first three items are within the limit ...
            ExceptionAssert.DoesNotThrow(() =>
            {
                context.IncrementSelectExpandNodeCount();
                context.IncrementSelectExpandNodeCount();
                context.IncrementSelectExpandNodeCount();
            });

            // ... while the fourth exceeds it.
            ExceptionAssert.Throws<ODataException>(
                () => context.IncrementSelectExpandNodeCount(),
                String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 3, "MaxSelectExpandNodeCount"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void IncrementSelectExpandNodeCount_IsNoOp_WhenCheckIsDisabled(int maxSelectExpandNodeCount)
        {
            // Arrange: a non-positive maximum disables the check.
            SelectExpandValidatorContext context = new SelectExpandValidatorContext();
            context.InitializeSelectExpandNodeCount(maxSelectExpandNodeCount);

            // Act & Assert: incrementing never throws, regardless of how many items are counted.
            ExceptionAssert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    context.IncrementSelectExpandNodeCount();
                }
            });
        }

        [Fact]
        public void IncrementSelectExpandNodeCount_IsNoOp_WhenNotInitialized()
        {
            // Arrange: a context whose counter was never initialized behaves as if the check is disabled.
            SelectExpandValidatorContext context = new SelectExpandValidatorContext();

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => context.IncrementSelectExpandNodeCount());
        }

        [Fact]
        public void Clone_SharesSelectExpandNodeCount_AcrossLevels()
        {
            // Arrange: the counter must accumulate across the whole projection tree, so a cloned sub-context
            // (produced for each nested $select/$expand level) shares the same running count and limit as its parent.
            SelectExpandValidatorContext root = new SelectExpandValidatorContext();
            root.InitializeSelectExpandNodeCount(3);
            SelectExpandValidatorContext nested = root.Clone();

            // Act & Assert: two items counted on the root plus one on the clone reach the shared limit of 3 ...
            ExceptionAssert.DoesNotThrow(() =>
            {
                root.IncrementSelectExpandNodeCount();
                root.IncrementSelectExpandNodeCount();
                nested.IncrementSelectExpandNodeCount();
            });

            // ... and the next item, counted on the clone, exceeds the shared limit.
            ExceptionAssert.Throws<ODataException>(
                () => nested.IncrementSelectExpandNodeCount(),
                String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 3, "MaxSelectExpandNodeCount"));
        }

        [Fact]
        public void Clone_DisabledSelectExpandNodeCount_RemainsDisabled()
        {
            // Arrange: a disabled check on the root ...
            SelectExpandValidatorContext root = new SelectExpandValidatorContext();
            root.InitializeSelectExpandNodeCount(0);

            // Act: ... carries to cloned sub-contexts.
            SelectExpandValidatorContext nested = root.Clone();

            // Assert
            ExceptionAssert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    nested.IncrementSelectExpandNodeCount();
                }
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-2)]
        public void WithProjectionWeightForLevels_ReturnsSameContext_WhenLevelsDoesNotScale(long levels)
        {
            // Arrange: a level count of 1 or less means "no $levels multiplicity", so there is nothing to scale and the
            // method must return the same context (the common path allocates no clone).
            SelectExpandValidatorContext context = new SelectExpandValidatorContext();
            context.InitializeSelectExpandNodeCount(100);

            // Act
            SelectExpandValidatorContext result = context.WithProjectionWeightForLevels(levels);

            // Assert
            Assert.Same(context, result);
        }

        [Fact]
        public void WithProjectionWeightForLevels_ReturnsSameContext_WhenCheckIsDisabled()
        {
            // Arrange: with the projection-item check disabled (non-positive maximum) there is no counter to scale, so
            // even a genuine $levels multiplicity leaves the context unchanged.
            SelectExpandValidatorContext context = new SelectExpandValidatorContext();
            context.InitializeSelectExpandNodeCount(0);

            // Act
            SelectExpandValidatorContext result = context.WithProjectionWeightForLevels(5);

            // Assert
            Assert.Same(context, result);
        }

        [Fact]
        public void WithProjectionWeightForLevels_ScalesWeight_SoEachIncrementCountsEveryLevel()
        {
            // Arrange: $levels=4 makes every counted item stand for four materialized copies, so each increment on the
            // scoped context adds 4. Against a maximum of 8, exactly two such increments fit (4, then 8).
            SelectExpandValidatorContext context = new SelectExpandValidatorContext();
            context.InitializeSelectExpandNodeCount(8);

            // Act
            SelectExpandValidatorContext scoped = context.WithProjectionWeightForLevels(4);

            // Assert: scaling clones, so the scoped context is a distinct instance ...
            Assert.NotSame(context, scoped);

            // ... and each increment counts 4, so two reach the limit of 8 and the third exceeds it.
            ExceptionAssert.DoesNotThrow(() =>
            {
                scoped.IncrementSelectExpandNodeCount();
                scoped.IncrementSelectExpandNodeCount();
            });
            ExceptionAssert.Throws<ODataException>(
                () => scoped.IncrementSelectExpandNodeCount(),
                String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 8, "MaxSelectExpandNodeCount"));
        }

        [Fact]
        public void WithProjectionWeightForLevels_CompoundsWeight_AcrossNestedLevels()
        {
            // Arrange: nested $levels multiply, so a $levels=3 subtree that itself contains a $levels=4 expansion counts
            // each innermost item 3 x 4 = 12 times. Scaling the already-scaled context reproduces that product.
            SelectExpandValidatorContext context = new SelectExpandValidatorContext();
            context.InitializeSelectExpandNodeCount(100);

            // Act
            SelectExpandValidatorContext scoped = context.WithProjectionWeightForLevels(3).WithProjectionWeightForLevels(4);

            // Assert: each increment now counts 12, so eight of them fit under 100 (96) and the ninth exceeds it.
            ExceptionAssert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 8; i++)
                {
                    scoped.IncrementSelectExpandNodeCount();
                }
            });
            ExceptionAssert.Throws<ODataException>(
                () => scoped.IncrementSelectExpandNodeCount(),
                String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 100, "MaxSelectExpandNodeCount"));
        }

        [Fact]
        public void WithProjectionWeightForLevels_ClampsScaledWeight_ToStayOverflowSafe()
        {
            // Arrange: a $levels value far larger than the limit must not overflow the counter. The scaled weight is
            // clamped to just past the maximum (max + 1 = 11), which still forces rejection on the very first counted item.
            SelectExpandValidatorContext context = new SelectExpandValidatorContext();
            context.InitializeSelectExpandNodeCount(10);

            // Act
            SelectExpandValidatorContext scoped = context.WithProjectionWeightForLevels(2000000000);

            // Assert: one increment already exceeds the limit, and the clamp keeps the arithmetic away from overflow.
            Assert.NotSame(context, scoped);
            ExceptionAssert.Throws<ODataException>(
                () => scoped.IncrementSelectExpandNodeCount(),
                String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 10, "MaxSelectExpandNodeCount"));
        }

        [Fact]
        public void WithProjectionWeightForLevels_Clones_SoSiblingItemsKeepTheirOwnWeight()
        {
            // Arrange: scaling for $levels must not leak onto sibling items counted through the original context. A
            // $levels=4 expansion counts 4 per item, while siblings counted on the original context still count 1.
            SelectExpandValidatorContext context = new SelectExpandValidatorContext();
            context.InitializeSelectExpandNodeCount(10);

            // Act
            SelectExpandValidatorContext scoped = context.WithProjectionWeightForLevels(4);

            // Assert: two scoped increments (4 + 4 = 8) plus two sibling increments on the original (1 + 1 = 10) reach the
            // limit exactly; a further sibling increment exceeds it. If the weight had leaked, the siblings would count 4.
            Assert.NotSame(context, scoped);
            ExceptionAssert.DoesNotThrow(() =>
            {
                scoped.IncrementSelectExpandNodeCount();
                scoped.IncrementSelectExpandNodeCount();
                context.IncrementSelectExpandNodeCount();
                context.IncrementSelectExpandNodeCount();
            });
            ExceptionAssert.Throws<ODataException>(
                () => context.IncrementSelectExpandNodeCount(),
                String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 10, "MaxSelectExpandNodeCount"));
        }
    }
}
