import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface Post {
  id: number;
  content: string;
  authorId: string;
  authorName: string;
  isModeratorPost: boolean;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class PostsService {
  private http = inject(HttpClient);
  private api = environment.apiUrl;

  getPosts() {
    return this.http.get<Post[]>(`${this.api}/api/posts`);
  }

  createPost(content: string) {
    return this.http.post<Post>(`${this.api}/api/posts`, { content });
  }

  deletePost(id: number) {
    return this.http.delete(`${this.api}/api/posts/${id}`);
  }
}
