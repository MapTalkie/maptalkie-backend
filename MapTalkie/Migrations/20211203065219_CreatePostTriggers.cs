using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MapTalkie.Migrations
{
    public partial class CreatePostTriggers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
create or replace function on_post_with_shared_created() returns trigger as
$$
begin
    update posts
    set cached_shares_count = cached_shares_count + 1
    where id = new.shared_id;
    return new;
end;
$$ language plpgsql;

create trigger tg_post_with_shared_created
    before insert 
    on posts
    for each row 
    when ( new.shared_id is not null and new.available )
    execute function on_post_with_shared_created();

create or replace function on_post_with_shared_updated() returns trigger as
$$
declare 
    change int;
begin
    if new.available != old.available then
        if new.available then
            change := 1;
        else
            change := -1;
        end if;

        update posts
        set cached_shares_count = cached_shares_count + change
        where id = new.shared_id;
    end if;
    return new;
end;
$$ language plpgsql;

create trigger tg_post_with_shared_updated
    before update 
    on posts
    for each row
    when ( new.shared_id is not null )
    execute function on_post_with_shared_updated();

create or replace function on_post_with_shared_deleted() returns trigger as
$$
begin
    update posts
    set cached_shares_count = cached_shares_count - 1
    where id = old.shared_id;
    return old;
end;
$$ language plpgsql;

create trigger tg_post_with_shared_deleted
    before delete 
    on posts
    for each row
    when ( old.shared_id is not null )
    execute function on_post_with_shared_deleted();

create or replace function on_post_comment_created() returns trigger as
$$
begin
    update posts
    set cached_comments_count = cached_comments_count + 1
    where id = new.post_id;
    return new;
end;
$$ language plpgsql;

create trigger tg_post_comment_created
    before insert 
    on post_comments
    for each row
    when ( new.available )
    execute function on_post_comment_created();

create or replace function on_post_comment_deleted() returns trigger as
$$
begin
    update posts
    set cached_comments_count = cached_comments_count - 1
    where id = old.post_id;
    return old;
end;
$$ language plpgsql;

create trigger tg_post_comment_deleted
    before delete 
    on post_comments
    execute function on_post_comment_deleted();

create or replace function on_post_comment_updated() returns trigger as
$$
declare 
    change int;
begin
    if new.available != old.available then
        if new.available then
            change := 1;
        else
            change := -1;
        end if;

        update posts
        set cached_comments_count = cached_comments_count + change
        where id = new.post_id;
    end if;
    return new;
end;
$$ language plpgsql;

create trigger tg_post_comment_updated
    before update 
    on post_comments
    for each row
    execute function on_post_comment_updated();

create or replace function on_like_created() returns trigger as
$$
begin
    update posts
    set cached_likes_count = cached_likes_count + 1
    where id = new.post_id;
    return new;
end;
$$ language plpgsql;

create trigger tg_like_created
    before insert 
    on post_likes
    for each row
    execute procedure on_like_created();

create or replace function on_like_deleted() returns trigger as
$$
begin
    update posts
    set cached_likes_count = cached_likes_count - 1
    where id = old.post_id;
    return old;
end;
$$ language plpgsql;

create trigger tg_like_deleted
    before delete 
    on post_likes
    for each row
    execute procedure on_like_deleted();
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
drop trigger if exists tg_like_created on post_likes;
drop trigger if exists tg_like_deleted on post_likes;
drop trigger if exists tg_post_comment_created on post_comments;
drop trigger if exists tg_post_comment_deleted on post_comments;
drop trigger if exists tg_post_comment_updated on post_comments;
drop trigger if exists tg_post_with_shared_created on posts;
drop trigger if exists tg_post_with_shared_updated on posts;
drop trigger if exists tg_post_with_shared_deleted on posts;
");
        }
    }
}
