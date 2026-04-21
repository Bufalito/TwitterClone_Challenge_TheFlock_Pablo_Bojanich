using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_tweet_id",
                table: "tweets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tweets_parent_tweet_id",
                table: "tweets",
                column: "parent_tweet_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tweets_tweets_parent_tweet_id",
                table: "tweets",
                column: "parent_tweet_id",
                principalTable: "tweets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tweets_tweets_parent_tweet_id",
                table: "tweets");

            migrationBuilder.DropIndex(
                name: "IX_tweets_parent_tweet_id",
                table: "tweets");

            migrationBuilder.DropColumn(
                name: "parent_tweet_id",
                table: "tweets");
        }
    }
}
