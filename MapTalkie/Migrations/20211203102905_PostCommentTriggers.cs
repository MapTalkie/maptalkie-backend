using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MapTalkie.Migrations
{
    public partial class PostCommentTriggers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
create or replace function on_post_comment_like_added() returns trigger language plpgsql as $$
begin 
    update post_comments
        set cached_likes_count = cached_likes_count + 1
        where id = new.post_id;
    return new;
end;
$$;

create trigger tg_post_comment_like_added
    before insert 
    on post_comments
    execute function on_post_comment_like_added();

create or replace function on_post_comment_like_deleted() returns trigger language plpgsql as $$
begin
    update post_comments
    set cached_likes_count = cached_likes_count - 1
    where id = new.post_id;
    return old;
end;
$$;

create trigger tg_post_comment_like_deleted
    before insert
    on post_comments
    execute function on_post_comment_like_deleted();
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
drop trigger tg_post_comment_like_deleted on post_comments;
drop trigger tg_post_comment_like_added on post_comments;
drop function on_post_comment_like_deleted;
drop function on_post_comment_like_added;
");
        }
    }
}
