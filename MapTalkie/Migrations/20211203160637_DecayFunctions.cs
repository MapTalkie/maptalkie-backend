using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MapTalkie.Migrations
{
    public partial class DecayFunctions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
create or replace function calculate_exp_content_decay(
    parameter double precision, 
    created_at timestamp with time zone) returns double precision language plpgsql as $$
    begin 
        return exp(-power(-(extract(epoch from now()) - extract(epoch from created_at)) / 86400, 2.0) * parameter);
    end;
    $$;

create or replace procedure update_exp_ranking_decay_of_post_comments(
    min_decay_allowed double precision,
    parameter double precision) language plpgsql as 
$$
    declare
        now_value timestamp with time zone;
        time_lower_bound timestamp with time zone;
        max_day_before_stop_decay double precision;
        total_days_in_epoch_now double precision;
    begin
        if min_decay_allowed >= 1 then
            min_decay_allowed := .99;
        end if;
        now_value := now() at time zone 'utc';
        total_days_in_epoch_now := extract(epoch from now_value) / 86400;
        max_day_before_stop_decay := sqrt(-ln(min_decay_allowed) / parameter);
        time_lower_bound := now_value - make_interval(secs := max_day_before_stop_decay * 86400);
        
        update post_comments c
            set rank_decay_factor = calculate_exp_content_decay(parameter, c.created_at)
            where c.created_at > time_lower_bound and c.created_at <= now_value;
    end;
$$;

create or replace procedure update_exp_ranking_decay(
    min_decay_allowed double precision,
    parameter double precision,
    self_decay_importance double precision = 1,
    like_decay_importance double precision = 1,
    share_decay_importance double precision = 1,
    comment_decay_importance double precision = 1) language plpgsql as
$$
    declare 
        now_value timestamp with time zone;
        time_lower_bound timestamp with time zone;
        max_day_before_stop_decay double precision;
        total_days_in_epoch_now double precision;
    begin 
        call update_exp_ranking_decay_of_post_comments(min_decay_allowed, parameter);
        if min_decay_allowed >= 1 then
            min_decay_allowed := .99;
        end if;
        now_value := now() at time zone 'utc';
        total_days_in_epoch_now := extract(epoch from now_value) / 86400;
        max_day_before_stop_decay := sqrt(-ln(min_decay_allowed) / parameter);
        time_lower_bound := now_value - make_interval(secs := max_day_before_stop_decay * 86400);
        
        -- репосты
        update posts as p
            set rank_decay_factor = (
                calculate_exp_content_decay(parameter, p.created_at) * self_decay_importance + (
                    select sum(calculate_exp_content_decay(parameter, pl.created_at))
                    from post_likes as pl
                    where pl.post_id = p.id
                ) * like_decay_importance + (
                    select sum(pc.rank_decay_factor)
                    from post_comments as pc
                    where pc.post_id = p.id
                ) * comment_decay_importance + (
                    select sum(p2.rank_decay_factor)
                    from posts as p2
                    where p2.shared_id = p.id
                ) * share_decay_importance
            ) / (self_decay_importance + 
                 p.cached_likes_count * like_decay_importance + 
                 p.cached_comments_count * comment_decay_importance + 
                 p.cached_shares_count + share_decay_importance)
            where available and shared_id is not null and created_at >= time_lower_bound;
        
        -- не-репосты
        update posts as p
            set rank_decay_factor = (
                calculate_exp_content_decay(parameter, p.created_at) + coalesce(
                    (select sum(calculate_exp_content_decay(parameter, pl.created_at))
                     from post_likes as pl
                     where pl.post_id = p.id),
                    0
                ) + coalesce(
                    (select sum(pc.rank_decay_factor)
                     from post_comments as pc
                     where pc.post_id = p.id),
                    0
                ) + coalesce(
                    (select sum(p2.rank_decay_factor)
                     from posts as p2
                     where p2.shared_id = p.id),
                    0
                )
            ) / (1 + p.cached_likes_count + p.cached_comments_count + p.cached_shares_count)
            where available and shared_id is null and created_at >= time_lower_bound;
    end;
$$;");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
drop procedure update_exp_ranking_decay(min_decay_allowed double precision, parameter double precision);
drop procedure update_exp_ranking_decay_of_post_comments(min_decay_allowed double precision, parameter double precision);
drop function calculate_exp_content_decay(parameter double precision, created_at timestamp with time zone);
");
        }
    }
}
