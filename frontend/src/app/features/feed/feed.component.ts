import { Component, OnInit, inject, signal } from '@angular/core';
import { NgFor, NgIf, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { PostsService, Post } from '../../core/posts.service';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-feed',
  standalone: true,
  imports: [NgFor, NgIf, DatePipe, ReactiveFormsModule],
  template: `
    <div class="layout">
      <header>
        <span class="logo">UgSocial</span>
        <div class="user-info">
          <span class="username">{{ auth.displayName }}</span>
          <span class="badge mod" *ngIf="auth.isModerator">Moderator</span>
          <button class="logout-btn" (click)="auth.logout()">Sign out</button>
        </div>
      </header>

      <main>
        <div class="compose-box">
          <textarea
            [formControl]="postContent"
            placeholder="What's on your mind?"
            rows="3"
            maxlength="500"
          ></textarea>
          <div class="compose-footer">
            <span class="char-count">{{ postContent.value?.length ?? 0 }} / 500</span>
            <button (click)="createPost()" [disabled]="posting || postContent.invalid">
              {{ posting ? 'Posting…' : 'Post' }}
            </button>
          </div>
          <div class="error" *ngIf="postError">{{ postError }}</div>
        </div>

        <div class="loading" *ngIf="loading()">Loading posts…</div>

        <div class="post-card" *ngFor="let post of posts()">
          <div class="post-header">
            <span class="author">{{ post.authorName }}</span>
            <span class="badge mod" *ngIf="post.isModeratorPost">Moderator</span>
            <span class="date">{{ post.createdAt | date:'medium' }}</span>
          </div>
          <p class="content">{{ post.content }}</p>
          <div class="post-actions" *ngIf="auth.isModerator">
            <button class="delete-btn" (click)="deletePost(post.id)">Delete</button>
          </div>
        </div>

        <div class="empty" *ngIf="!loading() && posts().length === 0">
          No posts yet. Be the first!
        </div>
      </main>
    </div>
  `,
  styles: [`
    * { box-sizing: border-box; }
    .layout { min-height: 100vh; background: #0f172a; color: #f1f5f9; }
    header {
      background: #1e293b; padding: 1rem 2rem;
      display: flex; align-items: center; justify-content: space-between;
      border-bottom: 1px solid #334155; position: sticky; top: 0; z-index: 10;
    }
    .logo { color: #60a5fa; font-size: 1.3rem; font-weight: bold; }
    .user-info { display: flex; align-items: center; gap: 0.75rem; }
    .username { color: #94a3b8; }
    .badge.mod { background: #7c3aed; color: white; padding: 0.2rem 0.6rem; border-radius: 999px; font-size: 0.75rem; }
    .logout-btn { background: transparent; border: 1px solid #475569; color: #94a3b8; padding: 0.35rem 0.8rem; border-radius: 6px; cursor: pointer; }
    .logout-btn:hover { border-color: #60a5fa; color: #60a5fa; }
    main { max-width: 640px; margin: 0 auto; padding: 2rem 1rem; }
    .compose-box { background: #1e293b; border-radius: 12px; padding: 1.2rem; margin-bottom: 1.5rem; border: 1px solid #334155; }
    textarea { width: 100%; background: #0f172a; border: 1px solid #334155; color: #f1f5f9; border-radius: 8px; padding: 0.8rem; font-size: 0.95rem; resize: none; }
    textarea:focus { outline: none; border-color: #60a5fa; }
    .compose-footer { display: flex; justify-content: space-between; align-items: center; margin-top: 0.75rem; }
    .char-count { color: #64748b; font-size: 0.8rem; }
    .compose-footer button { background: #3b82f6; color: white; border: none; padding: 0.5rem 1.2rem; border-radius: 8px; cursor: pointer; }
    .compose-footer button:disabled { opacity: 0.5; cursor: not-allowed; }
    .compose-footer button:hover:not(:disabled) { background: #2563eb; }
    .error { color: #f87171; font-size: 0.85rem; margin-top: 0.5rem; }
    .loading, .empty { text-align: center; color: #64748b; padding: 2rem; }
    .post-card { background: #1e293b; border-radius: 12px; padding: 1.2rem; margin-bottom: 1rem; border: 1px solid #334155; }
    .post-header { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.75rem; flex-wrap: wrap; }
    .author { font-weight: 600; color: #f1f5f9; }
    .date { color: #64748b; font-size: 0.8rem; margin-left: auto; }
    .content { color: #cbd5e1; line-height: 1.6; margin: 0; }
    .post-actions { margin-top: 0.75rem; display: flex; justify-content: flex-end; }
    .delete-btn { background: transparent; border: 1px solid #ef4444; color: #ef4444; padding: 0.3rem 0.8rem; border-radius: 6px; cursor: pointer; font-size: 0.85rem; }
    .delete-btn:hover { background: #ef4444; color: white; }
  `]
})
export class FeedComponent implements OnInit {
  auth = inject(AuthService);
  private postsService = inject(PostsService);

  posts = signal<Post[]>([]);
  loading = signal(true);
  postContent = new FormControl('', [Validators.required, Validators.maxLength(500)]);
  posting = false;
  postError = '';

  ngOnInit() {
    this.loadPosts();
  }

  loadPosts() {
    this.loading.set(true);
    this.postsService.getPosts().subscribe({
      next: (posts) => {
        this.posts.set(posts);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  createPost() {
    const content = this.postContent.value?.trim();
    if (!content) return;
    this.posting = true;
    this.postError = '';

    this.postsService.createPost(content).subscribe({
      next: (post) => {
        this.posts.update(posts => [post, ...posts]);
        this.postContent.reset();
        this.posting = false;
      },
      error: (err) => {
        this.postError = err.error?.error ?? 'Failed to post.';
        this.posting = false;
      }
    });
  }

  deletePost(id: number) {
    this.postsService.deletePost(id).subscribe({
      next: () => this.posts.update(posts => posts.filter(p => p.id !== id))
    });
  }
}
