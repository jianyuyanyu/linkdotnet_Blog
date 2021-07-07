﻿using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LinkDotNet.Blog.TestUtilities;
using LinkDotNet.Infrastructure.Persistence.InMemory;
using Xunit;

namespace LinkDotNet.Blog.UnitTests.Infrastructure.Persistence.InMemory
{
    public class InMemoryRepositoryTests
    {
        private readonly InMemoryRepository sut;

        public InMemoryRepositoryTests()
        {
            sut = new InMemoryRepository();
        }

        [Fact]
        public async Task ShouldStoreAndRetrieveBlogPost()
        {
            var blogPost = new BlogPostBuilder().WithTitle("My Title").Build();
            await sut.StoreAsync(blogPost);

            var blogPostFromRepo = await sut.GetByIdAsync(blogPost.Id);

            blogPostFromRepo.Title.Should().Be("My Title");
        }

        [Fact]
        public async Task ShouldHandleUpdates()
        {
            var blogPost = new BlogPostBuilder().WithTitle("My Title").Build();
            var newerPost = new BlogPostBuilder().WithTitle("My new Title").Build();
            await sut.StoreAsync(blogPost);
            blogPost.Update(newerPost);

            await sut.StoreAsync(blogPost);

            var blogPostFromRepository = await sut.GetByIdAsync(blogPost.Id);
            blogPostFromRepository.Title.Should().Be("My new Title");
        }

        [Fact]
        public async Task ShouldFilterAndOrder()
        {
            var olderPost = new BlogPostBuilder().Build();
            var newerPost = new BlogPostBuilder().Build();
            var filteredOutPost = new BlogPostBuilder().WithTitle("FilterOut").Build();
            await sut.StoreAsync(olderPost);
            await sut.StoreAsync(newerPost);
            await sut.StoreAsync(filteredOutPost);

            var blogPosts = await sut.GetAllAsync(
                bp => bp.Title != "FilterOut",
                bp => bp.UpdatedDate,
                false);

            var retrievedPosts = blogPosts.ToList();
            retrievedPosts.Any(b => b.Id == filteredOutPost.Id).Should().BeFalse();
            retrievedPosts[0].Id.Should().Be(olderPost.Id);
            retrievedPosts[1].Id.Should().Be(newerPost.Id);
        }

        [Fact]
        public async Task ShouldDelete()
        {
            var blogPost = new BlogPostBuilder().Build();
            await sut.StoreAsync(blogPost);

            await sut.DeleteAsync(blogPost.Id);

            (await sut.GetByIdAsync(blogPost.Id)).Should().BeNull();
        }
    }
}